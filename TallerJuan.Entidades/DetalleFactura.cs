namespace TallerJuan.Entidades
{
    
    /// Línea de detalle de una factura. Es la tabla intermedia N:M entre FACTURA y
    /// PRODUCTO (la cuarta y última N:M del proyecto). La PK compuesta (factura, producto) impide
    /// repetir el mismo producto dentro de una factura. El subtotal es CANTIDAD * PRECIO_UNITARIO.
    
    public class DetalleFactura
    {
        /// Número de la factura a la que pertenece la línea (FK a FACTURA).
        public int FacturaNumeroFactura { get; set; }

        /// Código del producto de la línea (FK a PRODUCTO).
        public string ProductoCodigo { get; set; } = string.Empty;

        /// Nombre del producto (solo para mostrar).
        public string ProductoNombre { get; set; } = string.Empty;

        ///Identificador autonumérico de la línea.
        public int IdDetalle { get; set; }

        /// Descripción de la línea (opcional; por defecto el nombre del producto).
        public string Descripcion { get; set; } = string.Empty;

        /// Tipo de línea: SERVICIO o REPUESTO.
        public string Tipo { get; set; } = string.Empty;

        /// Cantidad de la línea (mayor a cero).
        public int Cantidad { get; set; }

        /// Precio unitario de la línea.
        public decimal PrecioUnitario { get; set; }

        /// Subtotal de la línea (CANTIDAD * PRECIO_UNITARIO).
        public decimal Subtotal { get; set; }
    }
}
