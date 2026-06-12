namespace TallerJuan.Entidades
{
    /// <summary>
    /// Fila del reporte de repuestos más utilizados (RF-40). Resume las salidas de stock
    /// (MOVIMIENTO_INVENTARIO TIPO = SALIDA) del período por producto.
    /// </summary>
    public class ReporteRepuestoUtilizado
    {
        /// <summary>Código del repuesto.</summary>
        public string Codigo { get; set; } = string.Empty;

        /// <summary>Nombre del repuesto.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Stock actual del repuesto (referencia).</summary>
        public int StockActual { get; set; }

        /// <summary>Cantidad total utilizada (salidas) en el período.</summary>
        public int CantidadUtilizada { get; set; }

        /// <summary>Número de órdenes distintas que usaron el repuesto.</summary>
        public int OrdenesDistintas { get; set; }
    }
}
