namespace TallerJuan.Entidades
{
    /// <summary>
    /// Representa un movimiento de inventario (Fase 5). Es la tabla intermedia N:M entre
    /// PRODUCTO y ORDEN_TRABAJO: toda SALIDA (repuesto usado) o INGRESO (devolución) se
    /// asocia siempre a una orden de trabajo y afecta el STOCK_ACTUAL del producto.
    /// </summary>
    public class MovimientoInventario
    {
        /// <summary>Identificador autonumérico del movimiento (parte de la PK compuesta).</summary>
        public int IdMovimiento { get; set; }

        /// <summary>Código del producto afectado (FK a PRODUCTO).</summary>
        public string ProductoCodigo { get; set; } = string.Empty;

        /// <summary>Nombre del producto (solo para mostrar en la lista).</summary>
        public string ProductoNombre { get; set; } = string.Empty;

        /// <summary>Número de la orden de trabajo asociada (FK a ORDEN_TRABAJO).</summary>
        public int OrdenTrabajoNumeroOrden { get; set; }

        /// <summary>Tipo de movimiento: INGRESO o SALIDA.</summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>Fecha y hora del movimiento.</summary>
        public DateTime Fecha { get; set; }

        /// <summary>Motivo del movimiento (opcional).</summary>
        public string Motivo { get; set; } = string.Empty;

        /// <summary>Cantidad movida (siempre mayor a cero).</summary>
        public int Cantidad { get; set; }

        /// <summary>Stock actual del producto tras el movimiento (solo para mostrar).</summary>
        public int StockActual { get; set; }
    }
}
