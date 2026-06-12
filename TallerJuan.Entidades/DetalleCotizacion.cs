namespace TallerJuan.Entidades
{
    
    /// Línea de detalle de una cotización (Fase 5). Es la tabla intermedia N:M entre
    /// COTIZACION y PRODUCTO. La PK compuesta (cotización, producto) impide repetir el mismo
    /// producto dentro de una cotización. El subtotal es CANTIDAD * PRECIO_UNITARIO.
    
    public class DetalleCotizacion
    {
        /// Número de la cotización a la que pertenece la línea (FK a COTIZACION).
        public int CotizacionNumeroCotizacion { get; set; }

        /// Código del producto de la línea (FK a PRODUCTO).
        public string ProductoCodigo { get; set; } = string.Empty;

        /// Nombre del producto (solo para mostrar).
        public string ProductoNombre { get; set; } = string.Empty;

        /// Identificador autonumérico de la línea.
        public int IdDetalleCotizacion { get; set; }

        ///Descripción de la línea (opcional; por defecto el nombre del producto).
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
