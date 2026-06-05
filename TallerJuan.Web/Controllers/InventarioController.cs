using Microsoft.AspNetCore.Mvc;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Inventario y Repuestos. En la Fase 2 solo muestra un placeholder "en construcción".
    /// </summary>
    public class InventarioController : Controller
    {
        public IActionResult Index()
        {
            return View("EnConstruccion", "Inventario y Repuestos");
        }
    }
}
