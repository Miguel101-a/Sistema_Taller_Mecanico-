using Microsoft.AspNetCore.Mvc;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Empleados. En la Fase 2 solo muestra un placeholder "en construcción".
    /// </summary>
    public class EmpleadosController : Controller
    {
        public IActionResult Index()
        {
            return View("EnConstruccion", "Empleados");
        }
    }
}
