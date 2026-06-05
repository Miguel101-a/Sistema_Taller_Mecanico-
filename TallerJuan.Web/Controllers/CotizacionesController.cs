using Microsoft.AspNetCore.Mvc;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Cotizaciones. En la Fase 2 solo muestra un placeholder "en construcción".
    /// </summary>
    public class CotizacionesController : Controller
    {
        public IActionResult Index()
        {
            return View("EnConstruccion", "Cotizaciones");
        }
    }
}
