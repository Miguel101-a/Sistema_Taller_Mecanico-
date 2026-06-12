namespace TallerJuan.Entidades
{
    /// <summary>
    /// Fila del reporte de servicios más solicitados (RF-38). Resume las líneas TIPO = SERVICIO
    /// de las facturas EMITIDAS del período.
    /// </summary>
    public class ReporteServicioSolicitado
    {
        /// <summary>Código del producto/servicio.</summary>
        public string Codigo { get; set; } = string.Empty;

        /// <summary>Nombre del producto/servicio.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Cantidad total de veces solicitado.</summary>
        public int VecesSolicitado { get; set; }

        /// <summary>Monto total facturado por ese servicio.</summary>
        public decimal MontoTotal { get; set; }
    }
}
