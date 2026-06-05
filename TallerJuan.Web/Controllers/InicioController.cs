using Microsoft.AspNetCore.Mvc;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Controlador de la página de bienvenida (dashboard) que se muestra tras el login.
    /// </summary>
    public class InicioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
