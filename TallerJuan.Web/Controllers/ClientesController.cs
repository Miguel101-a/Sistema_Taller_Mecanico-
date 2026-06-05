using Microsoft.AspNetCore.Mvc;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Clientes. En la Fase 2 solo muestra un placeholder "en construcción".
    /// </summary>
    public class ClientesController : Controller
    {
        public IActionResult Index()
        {
            return View("EnConstruccion", "Clientes");
        }
    }
}
