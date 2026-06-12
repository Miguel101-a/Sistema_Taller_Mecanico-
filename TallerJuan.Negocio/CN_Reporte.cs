using TallerJuan.Datos;
using TallerJuan.Entidades;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Capa de Negocio para los 5 reportes del ERS (RF-37 a RF-41). Valida el rango de
    /// fechas (inicio ≤ fin) y la agrupación (DIA/MES/ANIO) antes de delegar en la capa de Datos.
    /// Solo lectura: no registra auditoría.
    /// </summary>
    public class CN_Reporte
    {
        /// <summary>Agrupaciones válidas para el reporte de ingresos por período.</summary>
        public static readonly string[] AgrupacionesValidas = { "DIA", "MES", "ANIO" };

        private readonly CD_Reporte _datos = new CD_Reporte();

        /// <summary>RF-37: ingresos por período (DIA/MES/ANIO) sobre facturas EMITIDAS.</summary>
        public List<ReporteIngresoPeriodo> IngresosPorPeriodo(DateTime fechaInicio, DateTime fechaFin, string agrupacion)
        {
            ValidarRango(fechaInicio, fechaFin);

            agrupacion = (agrupacion ?? string.Empty).Trim().ToUpperInvariant();
            if (!AgrupacionesValidas.Contains(agrupacion))
                throw new InvalidOperationException("La agrupación debe ser DIA, MES o ANIO.");

            return _datos.IngresosPorPeriodo(fechaInicio, fechaFin, agrupacion);
        }

        /// <summary>RF-38: servicios más solicitados en el período.</summary>
        public List<ReporteServicioSolicitado> ServiciosMasSolicitados(DateTime fechaInicio, DateTime fechaFin)
        {
            ValidarRango(fechaInicio, fechaFin);
            return _datos.ServiciosMasSolicitados(fechaInicio, fechaFin);
        }

        /// <summary>RF-39: productividad por mecánico en el período.</summary>
        public List<ReporteProductividadMecanico> ProductividadMecanico(DateTime fechaInicio, DateTime fechaFin)
        {
            ValidarRango(fechaInicio, fechaFin);
            return _datos.ProductividadMecanico(fechaInicio, fechaFin);
        }

        /// <summary>RF-40: repuestos más utilizados en el período.</summary>
        public List<ReporteRepuestoUtilizado> RepuestosMasUtilizados(DateTime fechaInicio, DateTime fechaFin)
        {
            ValidarRango(fechaInicio, fechaFin);
            return _datos.RepuestosMasUtilizados(fechaInicio, fechaFin);
        }

        /// <summary>RF-41: clientes frecuentes en el período.</summary>
        public List<ReporteClienteFrecuente> ClientesFrecuentes(DateTime fechaInicio, DateTime fechaFin)
        {
            ValidarRango(fechaInicio, fechaFin);
            return _datos.ClientesFrecuentes(fechaInicio, fechaFin);
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Valida que la fecha de inicio no sea posterior a la de fin.</summary>
        private static void ValidarRango(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaInicio.Date > fechaFin.Date)
                throw new InvalidOperationException("La fecha de inicio no puede ser posterior a la fecha de fin.");
        }
    }
}
