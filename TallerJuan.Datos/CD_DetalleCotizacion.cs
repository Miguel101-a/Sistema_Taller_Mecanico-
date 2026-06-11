using System.Data;
using Microsoft.Data.SqlClient;
using TallerJuan.Entidades;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Capa de Datos para el detalle de Cotizaciones (Fase 5 -> N:M COTIZACION ↔ PRODUCTO).
    /// Acceso solo por PROCEDIMIENTOS ALMACENADOS (ADO.NET). PROHIBIDO Entity Framework.
    /// El SP de inserción puede lanzar SqlException 2627/2601 si se repite el producto (PK compuesta).
    /// </summary>
    public class CD_DetalleCotizacion
    {
        /// <summary>Ejecuta sp_DetalleCotizacion_PorCotizacion y devuelve las líneas de una cotización.</summary>
        public List<DetalleCotizacion> ListarPorCotizacion(int numeroCotizacion)
        {
            List<DetalleCotizacion> detalles = new List<DetalleCotizacion>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_DetalleCotizacion_PorCotizacion", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroCotizacion", SqlDbType.Int) { Value = numeroCotizacion });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
                detalles.Add(Mapear(lector));

            return detalles;
        }

        /// <summary>
        /// Inserta una línea mediante sp_DetalleCotizacion_Insertar (el SUBTOTAL lo calcula el SP).
        /// Puede lanzar SqlException 2627/2601 si el producto ya existe en la cotización (PK compuesta);
        /// la capa de Negocio lo traduce.
        /// </summary>
        public void Insertar(DetalleCotizacion detalle)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_DetalleCotizacion_Insertar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroCotizacion", SqlDbType.Int) { Value = detalle.CotizacionNumeroCotizacion });
            comando.Parameters.Add(new SqlParameter("@ProductoCodigo", SqlDbType.VarChar, 20) { Value = detalle.ProductoCodigo });
            comando.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.VarChar, 200) { Value = ValorONulo(detalle.Descripcion) });
            comando.Parameters.Add(new SqlParameter("@Tipo", SqlDbType.VarChar, 20) { Value = detalle.Tipo });
            comando.Parameters.Add(new SqlParameter("@Cantidad", SqlDbType.Int) { Value = detalle.Cantidad });
            comando.Parameters.Add(new SqlParameter("@PrecioUnitario", SqlDbType.Decimal) { Value = detalle.PrecioUnitario });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>Elimina una línea (por cotización + producto) mediante sp_DetalleCotizacion_Eliminar.</summary>
        public void Eliminar(int numeroCotizacion, string productoCodigo)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_DetalleCotizacion_Eliminar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroCotizacion", SqlDbType.Int) { Value = numeroCotizacion });
            comando.Parameters.Add(new SqlParameter("@ProductoCodigo", SqlDbType.VarChar, 20) { Value = productoCodigo });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Mapea la fila actual del lector a un objeto DetalleCotizacion.</summary>
        private static DetalleCotizacion Mapear(SqlDataReader lector)
        {
            return new DetalleCotizacion
            {
                CotizacionNumeroCotizacion = LectorBD.Entero(lector, "COTIZACION_NUMERO_COTIZACION"),
                ProductoCodigo = LectorBD.Texto(lector, "PRODUCTO_CODIGO"),
                ProductoNombre = LectorBD.Texto(lector, "PRODUCTO_NOMBRE"),
                IdDetalleCotizacion = LectorBD.Entero(lector, "ID_DETALLE_COTIZACION"),
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
