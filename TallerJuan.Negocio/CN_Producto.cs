using Microsoft.Data.SqlClient;
using TallerJuan.Datos;
using TallerJuan.Entidades;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Capa de Negocio para Productos/Repuestos. El stock se moverá en la Fase 5;
    /// aquí solo se define al crear. Valida, traduce errores de BD y registra la auditoría.
    /// </summary>
    public class CN_Producto
    {
        private const string Modulo = "Inventario";
        private const int ErrorUniqueIndex = 2601;
        private const int ErrorUniqueConstraint = 2627;

        private readonly CD_Producto _datos = new CD_Producto();
        private readonly CD_Acceso _acceso = new CD_Acceso();

        /// <summary>Devuelve todos los productos.</summary>
        public List<Producto> Listar() => _datos.Listar();

        /// <summary>Devuelve un producto por su código, o null si no existe.</summary>
        public Producto? Obtener(string codigo) => _datos.Obtener(codigo);

        /// <summary>Devuelve los productos en o por debajo del stock mínimo (para la alerta del inventario).</summary>
        public List<Producto> AlertasStockMinimo() => _datos.AlertasStockMinimo();

        /// <summary>Crea un producto nuevo. Valida, traduce el código duplicado y registra la auditoría.</summary>
        public void Crear(Producto producto, string usuarioAccion)
        {
            Normalizar(producto);
            Validar(producto);

            try
            {
                _datos.Insertar(producto);
                _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Crear producto {producto.Codigo}");
            }
            catch (SqlException ex) when (EsDuplicado(ex))
            {
                throw new InvalidOperationException("Ya existe un producto con ese código.");
            }
        }

        /// <summary>Actualiza un producto existente (sin tocar STOCK_ACTUAL). Valida y registra la auditoría.</summary>
        public void Editar(Producto producto, string usuarioAccion)
        {
            Normalizar(producto);
            Validar(producto);

            _datos.Actualizar(producto);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Editar producto {producto.Codigo}");
        }

        /// <summary>Desactiva un producto (eliminación lógica) y registra la auditoría.</summary>
        public void Eliminar(string codigo, string usuarioAccion)
        {
            _datos.Eliminar(codigo);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Desactivar producto {codigo}");
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Recorta los espacios de los campos de texto.</summary>
        private static void Normalizar(Producto p)
        {
            p.Codigo = (p.Codigo ?? string.Empty).Trim();
            p.Nombre = (p.Nombre ?? string.Empty).Trim();
            p.Categoria = (p.Categoria ?? string.Empty).Trim();
            p.Marca = (p.Marca ?? string.Empty).Trim();
            p.Descripcion = (p.Descripcion ?? string.Empty).Trim();
        }

        /// <summary>Valida los campos obligatorios y los rangos numéricos de un producto.</summary>
        private static void Validar(Producto p)
        {
            if (string.IsNullOrWhiteSpace(p.Codigo))
                throw new InvalidOperationException("El código del producto es obligatorio.");
            if (string.IsNullOrWhiteSpace(p.Nombre))
                throw new InvalidOperationException("El nombre del producto es obligatorio.");
            if (p.StockActual < 0)
                throw new InvalidOperationException("El stock actual no puede ser negativo.");
            if (p.StockMinimo < 0)
                throw new InvalidOperationException("El stock mínimo no puede ser negativo.");
            if (p.PrecioCompra.HasValue && p.PrecioCompra < 0)
                throw new InvalidOperationException("El precio de compra no puede ser negativo.");
            if (p.PrecioVenta < 0)
                throw new InvalidOperationException("El precio de venta no puede ser negativo.");
        }

        /// <summary>Detecta violación de clave primaria/única (código duplicado).</summary>
        private static bool EsDuplicado(SqlException ex) =>
            ex.Number == ErrorUniqueConstraint || ex.Number == ErrorUniqueIndex;
    }
}
