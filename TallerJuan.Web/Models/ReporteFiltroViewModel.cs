namespace TallerJuan.Web.Models
{
    /// <summary>
    /// Filtro común de los reportes del ERS: rango de fechas (desde/hasta) y, solo para el reporte
    /// de ingresos, la agrupación (DIA/MES/ANIO). Por defecto cubre del primer día del mes actual a
    /// hoy. Los resultados de cada reporte se pasan a la vista por ViewBag.
    /// </summary>
    public class ReporteFiltroViewModel
    {
        /// <summary>Fecha de inicio del rango.</summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>Fecha de fin del rango.</summary>
        public DateTime FechaFin { get; set; }

        /// <summary>Agrupación del reporte de ingresos: DIA / MES / ANIO.</summary>
        public string Agrupacion { get; set; } = "DIA";

        /// <summary>Crea el filtro por defecto: del primer día del mes actual a hoy.</summary>
        public static ReporteFiltroViewModel PorDefecto() => new ReporteFiltroViewModel
        {
            FechaInicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
            FechaFin = DateTime.Today,
            Agrupacion = "DIA"
        };
    }
}
