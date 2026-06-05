using Microsoft.AspNetCore.Mvc;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Seguridad (Roles y Permisos). En la Fase 2 solo muestra un placeholder.
    /// La gestión real de roles y permisos se implementará en la Fase 3.
    /// </summary>
    public class SeguridadController : Controller
    {
        public IActionResult Index()
        {
            return View("EnConstruccion", "Seguridad (Roles y Permisos)");
        }
    }
}
