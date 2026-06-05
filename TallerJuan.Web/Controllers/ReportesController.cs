using Microsoft.AspNetCore.Mvc;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Reportes y Estadísticas. En la Fase 2 solo muestra un placeholder "en construcción".
    /// </summary>
    public class ReportesController : Controller
    {
        public IActionResult Index()
        {
            return View("EnConstruccion", "Reportes y Estadísticas");
        }
    }
}
