namespace TallerJuan.Entidades
{
    
    /// Representa un movimiento de inventario. Es la tabla intermedia N:M entre
    /// PRODUCTO y ORDEN_TRABAJO: toda SALIDA (repuesto usado) o INGRESO (devolución) se
    /// asocia siempre a una orden de trabajo y afecta el STOCK_ACTUAL del producto.
    
    public class MovimientoInventario
    {
        /// Identificador autonumérico del movimiento (parte de la PK compuesta).
        public int IdMovimiento { get; set; }

        /// Código del producto afectado (FK a PRODUCTO).
        public string ProductoCodigo { get; set; } = string.Empty;

        /// Nombre del producto (solo para mostrar en la lista).
        public string ProductoNombre { get; set; } = string.Empty;

        /// Número de la orden de trabajo asociada (FK a ORDEN_TRABAJO).
        public int OrdenTrabajoNumeroOrden { get; set; }

        /// Tipo de movimiento: INGRESO o SALIDA.
        public string Tipo { get; set; } = string.Empty;

        /// Fecha y hora del movimiento.
        public DateTime Fecha { get; set; }

        /// Motivo del movimiento (opcional).
        public string Motivo { get; set; } = string.Empty;

        /// Cantidad movida (siempre mayor a cero).
        public int Cantidad { get; set; }

        /// Stock actual del producto tras el movimiento (solo para mostrar).
        public int StockActual { get; set; }
    }
}
