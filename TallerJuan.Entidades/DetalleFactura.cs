namespace TallerJuan.Entidades
{
    /// <summary>
    /// Línea de detalle de una factura (Fase 6). Es la tabla intermedia N:M entre FACTURA y
    /// PRODUCTO (la cuarta y última N:M del proyecto). La PK compuesta (factura, producto) impide
    /// repetir el mismo producto dentro de una factura. El subtotal es CANTIDAD * PRECIO_UNITARIO.
    /// </summary>
    public class DetalleFactura
    {
        /// <summary>Número de la factura a la que pertenece la línea (FK a FACTURA).</summary>
        public int FacturaNumeroFactura { get; set; }

        /// <summary>Código del producto de la línea (FK a PRODUCTO).</summary>
        public string ProductoCodigo { get; set; } = string.Empty;

        /// <summary>Nombre del producto (solo para mostrar).</summary>
        public string ProductoNombre { get; set; } = string.Empty;

        /// <summary>Identificador autonumérico de la línea.</summary>
        public int IdDetalle { get; set; }

        /// <summary>Descripción de la línea (opcional; por defecto el nombre del producto).</summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Tipo de línea: SERVICIO o REPUESTO.</summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>Cantidad de la línea (mayor a cero).</summary>
        public int Cantidad { get; set; }

        /// <summary>Precio unitario de la línea.</summary>
        public decimal PrecioUnitario { get; set; }

        /// <summary>Subtotal de la línea (CANTIDAD * PRECIO_UNITARIO).</summary>
        public decimal Subtotal { get; set; }
    }
}
