using Microsoft.AspNetCore.Mvc;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Facturación. En la Fase 2 solo muestra un placeholder "en construcción".
    /// </summary>
    public class FacturacionController : Controller
    {
        public IActionResult Index()
        {
            return View("EnConstruccion", "Facturación");
        }
    }
}
