using System.Data;
using Microsoft.Data.SqlClient;
using TallerJuan.Entidades;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Capa de Datos para los Movimientos de Inventario (Fase 4 -> N:M PRODUCTO ↔ ORDEN_TRABAJO).
    /// Acceso solo por PROCEDIMIENTOS ALMACENADOS (ADO.NET). PROHIBIDO Entity Framework.
    /// El SP de inserción actualiza el STOCK_ACTUAL del producto en la misma transacción.
    /// </summary>
    public class CD_MovimientoInventario
    {
        /// <summary>Ejecuta sp_MovInventario_Listar y devuelve todos los movimientos (más recientes primero).</summary>
        public List<MovimientoInventario> Listar()
        {
            List<MovimientoInventario> movimientos = new List<MovimientoInventario>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_MovInventario_Listar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
                movimientos.Add(Mapear(lector));

            return movimientos;
        }

        /// <summary>Ejecuta sp_MovInventario_PorProducto y devuelve los movimientos de un producto.</summary>
        public List<MovimientoInventario> ListarPorProducto(string productoCodigo)
        {
            List<MovimientoInventario> movimientos = new List<MovimientoInventario>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_MovInventario_PorProducto", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@ProductoCodigo", SqlDbType.VarChar, 20) { Value = productoCodigo });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
                movimientos.Add(Mapear(lector));

            return movimientos;
        }

        /// <summary>
        /// Inserta un movimiento mediante sp_MovInventario_Insertar. El SP valida y actualiza el
        /// stock en una transacción; ante un problema (stock insuficiente, cantidad inválida, tipo
        /// inválido) lanza un SqlException que la capa de Negocio traduce al usuario.
        /// </summary>
        public void Insertar(MovimientoInventario movimiento)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_MovInventario_Insertar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@ProductoCodigo", SqlDbType.VarChar, 20) { Value = movimiento.ProductoCodigo });
            comando.Parameters.Add(new SqlParameter("@NumeroOrden", SqlDbType.Int) { Value = movimiento.OrdenTrabajoNumeroOrden });
            comando.Parameters.Add(new SqlParameter("@Tipo", SqlDbType.VarChar, 20) { Value = movimiento.Tipo });
            comando.Parameters.Add(new SqlParameter("@Motivo", SqlDbType.VarChar, 200) { Value = ValorONulo(movimiento.Motivo) });
            comando.Parameters.Add(new SqlParameter("@Cantidad", SqlDbType.Int) { Value = movimiento.Cantidad });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Mapea la fila actual del lector a un objeto MovimientoInventario.</summary>
        private static MovimientoInventario Mapear(SqlDataReader lector)
        {
            return new MovimientoInventario
            {
                IdMovimiento = LectorBD.Entero(lector, "ID_MOVIMIENTO"),
                ProductoCodigo = LectorBD.Texto(lector, "PRODUCTO_CODIGO"),
                ProductoNombre = LectorBD.Texto(lector, "PRODUCTO_NOMBRE"),
                OrdenTrabajoNumeroOrden = LectorBD.Entero(lector, "ORDEN_TRABAJO_NUMERO_ORDEN"),
                Tipo = LectorBD.Texto(lector, "TIPO"),
                Fecha = LectorBD.Fecha(lector, "FECHA"),
                Motivo = LectorBD.Texto(lector, "MOTIVO"),
                Cantidad = LectorBD.Entero(lector, "CANTIDAD"),
                StockActual = LectorBD.Entero(lector, "STOCK_ACTUAL")
            };
        }

        /// <summary>Convierte una cadena vacía en DBNull para columnas opcionales.</summary>
        private static object ValorONulo(string valor) =>
            string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
    }
}
