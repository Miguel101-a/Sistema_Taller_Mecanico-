using Microsoft.AspNetCore.Mvc;
using TallerJuan.Negocio;
using TallerJuan.Web.Models;
using TallerJuan.Web.Seguridad;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Reportes y Estadísticas (Fase 6). Expone los 5 reportes del ERS (RF-37 a RF-41):
    /// ingresos por período, servicios más solicitados, productividad por mecánico, repuestos más
    /// utilizados y clientes frecuentes. Todo bajo el permiso REPORTES_VER. Cada vista filtra por
    /// rango de fechas (por defecto, del primer día del mes actual a hoy) y muestra barras CSS.
    /// </summary>
    public class ReportesController : Controller
    {
        private readonly CN_Reporte _negocio = new CN_Reporte();

        private IActionResult? VerificarPermiso()
        {
            if (!SesionWeb.TienePermiso(HttpContext.Session, "REPORTES_VER"))
            {
                TempData["Error"] = "No tiene permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Inicio");
            }
            return null;
        }

        // ----------------------------------------------------------------------------------
        // Panel de reportes
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Index()
        {
            var redir = VerificarPermiso();
            if (redir != null) return redir;

            return View();
        }

        // ----------------------------------------------------------------------------------
        // RF-37: Ingresos por período
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Ingresos(DateTime? fechaInicio, DateTime? fechaFin, string? agrupacion)
        {
            var redir = VerificarPermiso();
            if (redir != null) return redir;

            var filtro = ResolverFiltro(fechaInicio, fechaFin, agrupacion);
            try
            {
                ViewBag.Resultados = _negocio.IngresosPorPeriodo(filtro.FechaInicio, filtro.FechaFin, filtro.Agrupacion);
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Resultados = new List<TallerJuan.Entidades.ReporteIngresoPeriodo>();
            }

            return View(filtro);
        }

        // ----------------------------------------------------------------------------------
        // RF-38: Servicios más solicitados
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Servicios(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var redir = VerificarPermiso();
            if (redir != null) return redir;

            var filtro = ResolverFiltro(fechaInicio, fechaFin, null);
            try
            {
                ViewBag.Resultados = _negocio.ServiciosMasSolicitados(filtro.FechaInicio, filtro.FechaFin);
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Resultados = new List<TallerJuan.Entidades.ReporteServicioSolicitado>();
            }

            return View(filtro);
        }

        // ----------------------------------------------------------------------------------
        // RF-39: Productividad por mecánico
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Productividad(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var redir = VerificarPermiso();
            if (redir != null) return redir;

            var filtro = ResolverFiltro(fechaInicio, fechaFin, null);
            try
            {
                ViewBag.Resultados = _negocio.ProductividadMecanico(filtro.FechaInicio, filtro.FechaFin);
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Resultados = new List<TallerJuan.Entidades.ReporteProductividadMecanico>();
            }

            return View(filtro);
        }

        // ----------------------------------------------------------------------------------
        // RF-40: Repuestos más utilizados
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Repuestos(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var redir = VerificarPermiso();
            if (redir != null) return redir;

            var filtro = ResolverFiltro(fechaInicio, fechaFin, null);
            try
            {
                ViewBag.Resultados = _negocio.RepuestosMasUtilizados(filtro.FechaInicio, filtro.FechaFin);
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Resultados = new List<TallerJuan.Entidades.ReporteRepuestoUtilizado>();
            }

            return View(filtro);
        }

        // ----------------------------------------------------------------------------------
        // RF-41: Clientes frecuentes
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult ClientesFrecuentes(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var redir = VerificarPermiso();
            if (redir != null) return redir;

            var filtro = ResolverFiltro(fechaInicio, fechaFin, null);
            try
            {
                ViewBag.Resultados = _negocio.ClientesFrecuentes(filtro.FechaInicio, filtro.FechaFin);
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Resultados = new List<TallerJuan.Entidades.ReporteClienteFrecuente>();
            }

            return View(filtro);
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// Construye el filtro a partir de los parámetros recibidos; si faltan fechas, usa el rango
        /// por defecto (primer día del mes actual a hoy). Normaliza la agrupación.
        /// </summary>
        private static ReporteFiltroViewModel ResolverFiltro(DateTime? fechaInicio, DateTime? fechaFin, string? agrupacion)
        {
            var defecto = ReporteFiltroViewModel.PorDefecto();
            return new ReporteFiltroViewModel
            {
                FechaInicio = fechaInicio ?? defecto.FechaInicio,
                FechaFin = fechaFin ?? defecto.FechaFin,
                Agrupacion = string.IsNullOrWhiteSpace(agrupacion) ? "DIA" : agrupacion.Trim().ToUpperInvariant()
            };
        }
    }
}
