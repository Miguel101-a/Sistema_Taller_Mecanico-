using Microsoft.AspNetCore.Mvc;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Vehículos. En la Fase 2 solo muestra un placeholder "en construcción".
    /// </summary>
    public class VehiculosController : Controller
    {
        public IActionResult Index()
        {
            return View("EnConstruccion", "Vehículos");
        }
    }
}
