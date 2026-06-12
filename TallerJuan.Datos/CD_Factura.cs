using System.Data;
using Microsoft.Data.SqlClient;
using TallerJuan.Entidades;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Capa de Datos para la cabecera de Facturas (Fase 6). Acceso solo por PROCEDIMIENTOS
    /// ALMACENADOS (ADO.NET). PROHIBIDO Entity Framework. El detalle (N:M con PRODUCTO) lo maneja
    /// CD_DetalleFactura. La inserción puede lanzar SqlException 2627/2601 si la orden ya tiene
    /// factura (UNIQUE sobre la FK -> relación 1:1); la capa de Negocio lo traduce.
    /// </summary>
    public class CD_Factura
    {
        /// <summary>
        /// Ejecuta sp_Orden_Facturables: órdenes FINALIZADO/ENTREGADO y sin factura previa (1:1).
        /// </summary>
        public List<OrdenTrabajo> ListarOrdenesFacturables()
        {
            List<OrdenTrabajo> ordenes = new List<OrdenTrabajo>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Orden_Facturables", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
            {
                ordenes.Add(new OrdenTrabajo
                {
                    NumeroOrden = LectorBD.Entero(lector, "NUMERO_ORDEN"),
                    FechaIngreso = LectorBD.Fecha(lector, "FECHA_INGRESO"),
                    Estado = LectorBD.Texto(lector, "ESTADO"),
                    VehiculoDescripcion = LectorBD.Texto(lector, "VEHICULO_DESCRIPCION"),
                    ClienteNombre = LectorBD.Texto(lector, "CLIENTE_NOMBRE")
                });
            }

            return ordenes;
        }

        /// <summary>Ejecuta sp_Factura_Listar (con datos de cliente y vehículo).</summary>
        public List<Factura> Listar()
        {
            List<Factura> facturas = new List<Factura>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Factura_Listar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
                facturas.Add(Mapear(lector));

            return facturas;
        }

        /// <summary>Ejecuta sp_Factura_Obtener y devuelve una factura por su número, o null.</summary>
        public Factura? Obtener(int numeroFactura)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Factura_Obtener", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroFactura", SqlDbType.Int) { Value = numeroFactura });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            return lector.Read() ? Mapear(lector) : null;
        }

        /// <summary>
        /// Inserta una factura en estado BORRADOR mediante sp_Factura_Insertar y devuelve el
        /// NUMERO_FACTURA generado (el SP hace SELECT SCOPE_IDENTITY()). Puede lanzar SqlException
        /// 2627/2601 si la orden ya tiene factura (UNIQUE 1:1).
        /// </summary>
        public int Insertar(int numeroOrden)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Factura_Insertar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroOrden", SqlDbType.Int) { Value = numeroOrden });

            conexion.Open();
            object? resultado = comando.ExecuteScalar();

            return resultado == null || resultado == DBNull.Value
                ? 0
                : Convert.ToInt32(resultado);
        }

        /// <summary>Cambia el estado de una factura mediante sp_Factura_CambiarEstado.</summary>
        public void CambiarEstado(int numeroFactura, string estado)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Factura_CambiarEstado", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroFactura", SqlDbType.Int) { Value = numeroFactura });
            comando.Parameters.Add(new SqlParameter("@Estado", SqlDbType.VarChar, 20) { Value = estado });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>
        /// Recalcula subtotal, IVA 13% y total de una factura mediante sp_Factura_RecalcularTotales.
        /// Se llama tras cada cambio en sus líneas.
        /// </summary>
        public void RecalcularTotales(int numeroFactura)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Factura_RecalcularTotales", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroFactura", SqlDbType.Int) { Value = numeroFactura });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>
        /// Ejecuta sp_Factura_RepuestosDeOrden: repuestos con salida neta positiva de la orden,
        /// listos para precargarse como líneas TIPO = REPUESTO (con su PRECIO_VENTA).
        /// </summary>
        public List<DetalleFactura> RepuestosDeOrden(int numeroOrden)
        {
            List<DetalleFactura> repuestos = new List<DetalleFactura>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Factura_RepuestosDeOrden", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroOrden", SqlDbType.Int) { Value = numeroOrden });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
            {
                repuestos.Add(new DetalleFactura
                {
                    ProductoCodigo = LectorBD.Texto(lector, "PRODUCTO_CODIGO"),
                    ProductoNombre = LectorBD.Texto(lector, "PRODUCTO_NOMBRE"),
                    Descripcion = LectorBD.Texto(lector, "PRODUCTO_NOMBRE"),
                    Tipo = "REPUESTO",
                    Cantidad = LectorBD.Entero(lector, "CANTIDAD_NETA"),
                    PrecioUnitario = LectorBD.Decimal(lector, "PRECIO_VENTA")
                });
            }

            return repuestos;
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Mapea la fila actual del lector a un objeto Factura.</summary>
        private static Factura Mapear(SqlDataReader lector)
        {
            return new Factura
            {
                NumeroFactura = LectorBD.Entero(lector, "NUMERO_FACTURA"),
                FechaEmision = LectorBD.Fecha(lector, "FECHA_EMISION"),
                Subtotal = LectorBD.Decimal(lector, "SUBTOTAL"),
                Iva = LectorBD.Decimal(lector, "IVA"),
                Total = LectorBD.Decimal(lector, "TOTAL"),
                Estado = LectorBD.Texto(lector, "ESTADO"),
                OrdenTrabajoNumeroOrden = LectorBD.Entero(lector, "ORDEN_TRABAJO_NUMERO_ORDEN"),
                VehiculoDescripcion = LectorBD.Texto(lector, "VEHICULO_DESCRIPCION"),
                ClienteCedula = LectorBD.Texto(lector, "CLIENTE_CEDULA"),
                ClienteNombre = LectorBD.Texto(lector, "CLIENTE_NOMBRE")
            };
        }
    }
}
