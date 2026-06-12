namespace TallerJuan.Entidades
{
    /// <summary>
    /// Fila del reporte de productividad por mecánico (RF-39). Resume las órdenes atendidas por
    /// cada mecánico en el período, cuántas terminó y su promedio de días de entrega.
    /// </summary>
    public class ReporteProductividadMecanico
    {
        /// <summary>Cédula del mecánico.</summary>
        public string Cedula { get; set; } = string.Empty;

        /// <summary>Nombre del mecánico.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Órdenes atendidas (asignadas) en el período.</summary>
        public int OrdenesAtendidas { get; set; }

        /// <summary>Órdenes finalizadas o entregadas.</summary>
        public int OrdenesTerminadas { get; set; }

        /// <summary>Promedio de días entre ingreso y entrega (null si no hay órdenes entregadas).</summary>
        public decimal? PromedioDiasEntrega { get; set; }
    }
}
