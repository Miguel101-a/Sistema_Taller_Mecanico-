namespace TallerJuan.Entidades
{
    /// <summary>
    /// Fila del reporte de ingresos por período (RF-37). Cada fila resume las facturas EMITIDAS
    /// de un período (día, mes o año, según la agrupación) con sus totales.
    /// </summary>
    public class ReporteIngresoPeriodo
    {
        /// <summary>Etiqueta del período (dd/MM/yyyy, MM/yyyy o yyyy según agrupación).</summary>
        public string Periodo { get; set; } = string.Empty;

        /// <summary>Fecha mínima del grupo (solo para ordenar cronológicamente).</summary>
        public DateTime FechaOrden { get; set; }

        /// <summary>Cantidad de facturas emitidas en el período.</summary>
        public int CantidadFacturas { get; set; }

        /// <summary>Suma de subtotales del período.</summary>
        public decimal Subtotal { get; set; }

        /// <summary>Suma del IVA del período.</summary>
        public decimal Iva { get; set; }

        /// <summary>Suma del total facturado del período.</summary>
        public decimal Total { get; set; }
    }
}
