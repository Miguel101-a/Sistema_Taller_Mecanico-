using System.Data;
using Microsoft.Data.SqlClient;
using TallerJuan.Entidades;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Capa de Datos para la cabecera de Cotizaciones. Acceso solo por PROCEDIMIENTOS
    /// ALMACENADOS (ADO.NET). PROHIBIDO Entity Framework. El detalle (N:M con PRODUCTO) lo
    /// maneja CD_DetalleCotizacion.
    /// </summary>
    public class CD_Cotizacion
    {
        /// <summary>Ejecuta sp_Cotizacion_Listar (con datos de cliente y vehículo).</summary>
        public List<Cotizacion> Listar()
        {
            List<Cotizacion> cotizaciones = new List<Cotizacion>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Cotizacion_Listar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
                cotizaciones.Add(Mapear(lector));

            return cotizaciones;
        }

        /// <summary>Ejecuta sp_Cotizacion_Obtener y devuelve una cotización por su número, o null.</summary>
        public Cotizacion? Obtener(int numeroCotizacion)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Cotizacion_Obtener", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroCotizacion", SqlDbType.Int) { Value = numeroCotizacion });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            return lector.Read() ? Mapear(lector) : null;
        }

        /// <summary>
        /// Inserta una cotización mediante sp_Cotizacion_Insertar y devuelve el NUMERO_COTIZACION
        /// generado (el SP hace SELECT SCOPE_IDENTITY()).
        /// </summary>
        public int Insertar(Cotizacion cotizacion)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Cotizacion_Insertar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@ValidezDias", SqlDbType.Int) { Value = (object?)cotizacion.ValidezDias ?? DBNull.Value });
            comando.Parameters.Add(new SqlParameter("@ClienteCedula", SqlDbType.VarChar, 20) { Value = cotizacion.ClienteCedula });
            comando.Parameters.Add(new SqlParameter("@VehiculoPlaca", SqlDbType.VarChar, 10) { Value = cotizacion.VehiculoPlaca });

            conexion.Open();
            object? resultado = comando.ExecuteScalar();

            return resultado == null || resultado == DBNull.Value
                ? 0
                : Convert.ToInt32(resultado);
        }

        /// <summary>Cambia el estado de una cotización mediante sp_Cotizacion_CambiarEstado.</summary>
        public void CambiarEstado(int numeroCotizacion, string estado)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Cotizacion_CambiarEstado", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroCotizacion", SqlDbType.Int) { Value = numeroCotizacion });
            comando.Parameters.Add(new SqlParameter("@Estado", SqlDbType.VarChar, 20) { Value = estado });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>
        /// Recalcula subtotales por tipo, IVA 13% y total de una cotización mediante
        /// sp_Cotizacion_RecalcularTotales. Se llama tras cada cambio en sus líneas.
        /// </summary>
        public void RecalcularTotales(int numeroCotizacion)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Cotizacion_RecalcularTotales", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroCotizacion", SqlDbType.Int) { Value = numeroCotizacion });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Mapea la fila actual del lector a un objeto Cotizacion.</summary>
        private static Cotizacion Mapear(SqlDataReader lector)
        {
            return new Cotizacion
            {
                NumeroCotizacion = LectorBD.Entero(lector, "NUMERO_COTIZACION"),
                FechaEmision = LectorBD.Fecha(lector, "FECHA_EMISION"),
                ValidezDias = LectorBD.EnteroNulo(lector, "VALIDEZ_DIAS"),
                SubtotalServicios = LectorBD.Decimal(lector, "SUBTOTAL_SERVICIOS"),
                SubtotalRepuestos = LectorBD.Decimal(lector, "SUBTOTAL_REPUESTOS"),
                Impuestos = LectorBD.Decimal(lector, "IMPUESTOS"),
                Total = LectorBD.Decimal(lector, "TOTAL"),
                Estado = LectorBD.Texto(lector, "ESTADO"),
                ClienteCedula = LectorBD.Texto(lector, "CLIENTE_CEDULA"),
                ClienteNombre = LectorBD.Texto(lector, "CLIENTE_NOMBRE"),
                VehiculoPlaca = LectorBD.Texto(lector, "VEHICULO_PLACA"),
                VehiculoDescripcion = LectorBD.Texto(lector, "VEHICULO_DESCRIPCION")
            };
        }
    }
}
