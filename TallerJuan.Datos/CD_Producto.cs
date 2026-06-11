using System.Data;
using Microsoft.Data.SqlClient;
using TallerJuan.Entidades;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Capa de Datos para el CRUD de Productos/Repuestos (Fase 4). El stock se moverá en la Fase 5;
    /// aquí solo se define al crear. Acceso solo por PROCEDIMIENTOS ALMACENADOS (ADO.NET).
    /// PROHIBIDO Entity Framework.
    /// </summary>
    public class CD_Producto
    {
        /// <summary>Ejecuta sp_Producto_Listar y devuelve todos los productos.</summary>
        public List<Producto> Listar()
        {
            List<Producto> productos = new List<Producto>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Producto_Listar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
                productos.Add(Mapear(lector));

            return productos;
        }

        /// <summary>Ejecuta sp_Producto_Obtener y devuelve un producto por su código, o null si no existe.</summary>
        public Producto? Obtener(string codigo)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Producto_Obtener", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Codigo", SqlDbType.VarChar, 20) { Value = codigo });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            return lector.Read() ? Mapear(lector) : null;
        }

        /// <summary>
        /// Ejecuta sp_Producto_AlertasStockMinimo y devuelve los productos en o por debajo del stock mínimo.
        /// </summary>
        public List<Producto> AlertasStockMinimo()
        {
            List<Producto> productos = new List<Producto>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Producto_AlertasStockMinimo", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
                productos.Add(Mapear(lector));

            return productos;
        }

        /// <summary>
        /// Inserta un producto mediante sp_Producto_Insertar. Puede lanzar SqlException 2627/2601
        /// si el código viola la clave primaria; la capa de Negocio lo traduce.
        /// </summary>
        public void Insertar(Producto producto)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Producto_Insertar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Codigo", SqlDbType.VarChar, 20) { Value = producto.Codigo });
            comando.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.VarChar, 100) { Value = producto.Nombre });
            comando.Parameters.Add(new SqlParameter("@Categoria", SqlDbType.VarChar, 50) { Value = ValorONulo(producto.Categoria) });
            comando.Parameters.Add(new SqlParameter("@Marca", SqlDbType.VarChar, 50) { Value = ValorONulo(producto.Marca) });
            comando.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.VarChar, 500) { Value = ValorONulo(producto.Descripcion) });
            comando.Parameters.Add(new SqlParameter("@StockActual", SqlDbType.Int) { Value = producto.StockActual });
            comando.Parameters.Add(new SqlParameter("@StockMinimo", SqlDbType.Int) { Value = producto.StockMinimo });
            comando.Parameters.Add(new SqlParameter("@PrecioCompra", SqlDbType.Decimal) { Value = (object?)producto.PrecioCompra ?? DBNull.Value });
            comando.Parameters.Add(new SqlParameter("@PrecioVenta", SqlDbType.Decimal) { Value = producto.PrecioVenta });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>
        /// Actualiza un producto mediante sp_Producto_Actualizar.
        /// NO actualiza STOCK_ACTUAL (el stock se moverá en la Fase 5).
        /// </summary>
        public void Actualizar(Producto producto)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Producto_Actualizar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Codigo", SqlDbType.VarChar, 20) { Value = producto.Codigo });
            comando.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.VarChar, 100) { Value = producto.Nombre });
            comando.Parameters.Add(new SqlParameter("@Categoria", SqlDbType.VarChar, 50) { Value = ValorONulo(producto.Categoria) });
            comando.Parameters.Add(new SqlParameter("@Marca", SqlDbType.VarChar, 50) { Value = ValorONulo(producto.Marca) });
            comando.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.VarChar, 500) { Value = ValorONulo(producto.Descripcion) });
            comando.Parameters.Add(new SqlParameter("@StockMinimo", SqlDbType.Int) { Value = producto.StockMinimo });
            comando.Parameters.Add(new SqlParameter("@PrecioCompra", SqlDbType.Decimal) { Value = (object?)producto.PrecioCompra ?? DBNull.Value });
            comando.Parameters.Add(new SqlParameter("@PrecioVenta", SqlDbType.Decimal) { Value = producto.PrecioVenta });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>Eliminación lógica de un producto (ESTADO='INACTIVO') mediante sp_Producto_Eliminar.</summary>
        public void Eliminar(string codigo)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Producto_Eliminar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Codigo", SqlDbType.VarChar, 20) { Value = codigo });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Mapea la fila actual del lector a un objeto Producto.</summary>
        private static Producto Mapear(SqlDataReader lector)
        {
            return new Producto
            {
                Codigo = LectorBD.Texto(lector, "CODIGO"),
                Nombre = LectorBD.Texto(lector, "NOMBRE"),
                Categoria = LectorBD.Texto(lector, "CATEGORIA"),
                Marca = LectorBD.Texto(lector, "MARCA"),
                Descripcion = LectorBD.Texto(lector, "DESCRIPCION"),
                StockActual = LectorBD.Entero(lector, "STOCK_ACTUAL"),
                StockMinimo = LectorBD.Entero(lector, "STOCK_MINIMO"),
                PrecioCompra = LectorBD.DecimalNulo(lector, "PRECIO_COMPRA"),
                PrecioVenta = LectorBD.Decimal(lector, "PRECIO_VENTA"),
                Estado = LectorBD.Texto(lector, "ESTADO")
            };
        }

        /// <summary>Convierte una cadena vacía en DBNull para columnas opcionales.</summary>
        private static object ValorONulo(string valor) =>
            string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
    }
}
