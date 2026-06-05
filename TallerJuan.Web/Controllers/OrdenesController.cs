using Microsoft.AspNetCore.Mvc;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Órdenes de Trabajo. En la Fase 2 solo muestra un placeholder "en construcción".
    /// </summary>
    public class OrdenesController : Controller
    {
        public IActionResult Index()
        {
            return View("EnConstruccion", "Órdenes de Trabajo");
        }
    }
}
