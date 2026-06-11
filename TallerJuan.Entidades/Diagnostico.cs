namespace TallerJuan.Entidades
{
    /// <summary>
    /// Representa un diagnóstico técnico registrado sobre una orden de trabajo.
    /// Una orden puede tener varios diagnósticos a lo largo del tiempo.
    /// </summary>
    public class Diagnostico
    {
        /// <summary>Identificador del diagnóstico (autoincremental).</summary>
        public int IdDiagnostico { get; set; }

        /// <summary>Descripción del diagnóstico (obligatoria).</summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Tiempo estimado de reparación en horas (puede ser nulo).</summary>
        public decimal? TiempoEstimado { get; set; }

        /// <summary>Costo estimado de la reparación (puede ser nulo).</summary>
        public decimal? CostoEstimado { get; set; }

        /// <summary>Fecha y hora del diagnóstico.</summary>
        public DateTime Fecha { get; set; }

        /// <summary>Número de la orden de trabajo asociada (clave foránea).</summary>
        public int OrdenTrabajoNumeroOrden { get; set; }
    }
}
