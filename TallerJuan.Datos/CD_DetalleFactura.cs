using System.Data;
using Microsoft.Data.SqlClient;
using TallerJuan.Entidades;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Capa de Datos para el detalle de Facturas (Fase 6 -> N:M FACTURA ↔ PRODUCTO, la cuarta y
    /// última N:M del proyecto). Acceso solo por PROCEDIMIENTOS ALMACENADOS (ADO.NET). PROHIBIDO
    /// Entity Framework. El SP de inserción puede lanzar SqlException 2627/2601 si se repite el
    /// producto (PK compuesta); la capa de Negocio lo traduce.
    /// </summary>
    public class CD_DetalleFactura
    {
        /// <summary>Ejecuta sp_DetalleFactura_PorFactura y devuelve las líneas de una factura.</summary>
        public List<DetalleFactura> ListarPorFactura(int numeroFactura)
        {
            List<DetalleFactura> detalles = new List<DetalleFactura>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_DetalleFactura_PorFactura", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroFactura", SqlDbType.Int) { Value = numeroFactura });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
                detalles.Add(Mapear(lector));

            return detalles;
        }

        /// <summary>
        /// Inserta una línea mediante sp_DetalleFactura_Insertar (el SUBTOTAL lo calcula el SP).
        /// Puede lanzar SqlException 2627/2601 si el producto ya existe en la factura (PK compuesta).
        /// </summary>
        public void Insertar(DetalleFactura detalle)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_DetalleFactura_Insertar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroFactura", SqlDbType.Int) { Value = detalle.FacturaNumeroFactura });
            comando.Parameters.Add(new SqlParameter("@ProductoCodigo", SqlDbType.VarChar, 20) { Value = detalle.ProductoCodigo });
            comando.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.VarChar, 200) { Value = ValorONulo(detalle.Descripcion) });
            comando.Parameters.Add(new SqlParameter("@Tipo", SqlDbType.VarChar, 20) { Value = detalle.Tipo });
            comando.Parameters.Add(new SqlParameter("@Cantidad", SqlDbType.Int) { Value = detalle.Cantidad });
            comando.Parameters.Add(new SqlParameter("@PrecioUnitario", SqlDbType.Decimal) { Value = detalle.PrecioUnitario });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>Elimina una línea (por factura + producto) mediante sp_DetalleFactura_Eliminar.</summary>
        public void Eliminar(int numeroFactura, string productoCodigo)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_DetalleFactura_Eliminar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroFactura", SqlDbType.Int) { Value = numeroFactura });
            comando.Parameters.Add(new SqlParameter("@ProductoCodigo", SqlDbType.VarChar, 20) { Value = productoCodigo });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Mapea la fila actual del lector a un objeto DetalleFactura.</summary>
        private static DetalleFactura Mapear(SqlDataReader lector)
        {
            return new DetalleFactura
            {
                FacturaNumeroFactura = LectorBD.Entero(lector, "FACTURA_NUMERO_FACTURA"),
                ProductoCodigo = LectorBD.Texto(lector, "PRODUCTO_CODIGO"),
                ProductoNombre = LectorBD.Texto(lector, "PRODUCTO_NOMBRE"),
                IdDetalle = LectorBD.Entero(lector, "ID_DETALLE"),
                Descripcion = LectorBD.Texto(lector, "DESCRIPCION"),
                Tipo = LectorBD.Texto(lector, "TIPO"),
                Cantidad = LectorBD.Entero(lector, "CANTIDAD"),
                PrecioUnitario = LectorBD.Decimal(lector, "PRECIO_UNITARIO"),
                Subtotal = LectorBD.Decimal(lector, "SUBTOTAL")
            };
        }

        /// <summary>Convierte una cadena vacía en DBNull para columnas opcionales.</summary>
        private static object ValorONulo(string valor) =>
            string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
    }
}
