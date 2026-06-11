namespace TallerJuan.Entidades
{
    /// <summary>
    /// Representa a un producto o repuesto del inventario del taller.
    /// El código es el identificador principal. El stock se moverá en la Fase 5;
    /// en esta fase solo se define al crear el producto.
    /// </summary>
    public class Producto
    {
        /// <summary>Código del producto (identificador principal).</summary>
        public string Codigo { get; set; } = string.Empty;

        /// <summary>Nombre del producto.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Categoría del producto.</summary>
        public string Categoria { get; set; } = string.Empty;

        /// <summary>Marca del producto.</summary>
        public string Marca { get; set; } = string.Empty;

        /// <summary>Descripción del producto.</summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Stock actual disponible.</summary>
        public int StockActual { get; set; }

        /// <summary>Stock mínimo antes de generar alerta de reposición.</summary>
        public int StockMinimo { get; set; }

        /// <summary>Precio de compra del producto (puede ser nulo).</summary>
        public decimal? PrecioCompra { get; set; }

        /// <summary>Precio de venta del producto.</summary>
        public decimal PrecioVenta { get; set; }

        /// <summary>Estado del producto (ACTIVO / INACTIVO).</summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Indica si el stock actual está en o por debajo del mínimo (alerta de reposición).</summary>
        public bool StockBajo => StockActual <= StockMinimo;
    }
}
