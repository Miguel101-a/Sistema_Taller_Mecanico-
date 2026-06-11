namespace TallerJuan.Entidades
{
    /// <summary>
    /// Línea de detalle de una cotización (Fase 5). Es la tabla intermedia N:M entre
    /// COTIZACION y PRODUCTO. La PK compuesta (cotización, producto) impide repetir el mismo
    /// producto dentro de una cotización. El subtotal es CANTIDAD * PRECIO_UNITARIO.
    /// </summary>
    public class DetalleCotizacion
    {
        /// <summary>Número de la cotización a la que pertenece la línea (FK a COTIZACION).</summary>
        public int CotizacionNumeroCotizacion { get; set; }

        /// <summary>Código del producto de la línea (FK a PRODUCTO).</summary>
        public string ProductoCodigo { get; set; } = string.Empty;

        /// <summary>Nombre del producto (solo para mostrar).</summary>
        public string ProductoNombre { get; set; } = string.Empty;

        /// <summary>Identificador autonumérico de la línea.</summary>
        public int IdDetalleCotizacion { get; set; }

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
