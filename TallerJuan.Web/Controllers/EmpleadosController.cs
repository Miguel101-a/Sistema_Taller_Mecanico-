using Microsoft.AspNetCore.Mvc;
using TallerJuan.Entidades;
using TallerJuan.Negocio;
using TallerJuan.Web.Models;
using TallerJuan.Web.Seguridad;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Empleados (Fase 4): CRUD completo con eliminación lógica. Al crear se asigna
    /// usuario, contraseña (hasheada en Negocio) y rol; al editar NO se cambian usuario ni
    /// contraseña. Un empleado no puede desactivarse a sí mismo. Permisos por acción y auditoría.
    /// </summary>
    public class EmpleadosController : Controller
    {
        private readonly CN_Empleado _negocio = new CN_Empleado();

        private IActionResult? VerificarPermiso(string clave)
        {
            if (!SesionWeb.TienePermiso(HttpContext.Session, clave))
            {
                TempData["Error"] = "No tiene permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Inicio");
            }
            return null;
        }

        private string UsuarioActual => SesionWeb.Usuario(HttpContext.Session);

        // ----------------------------------------------------------------------------------
        // Listado
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Index()
        {
            var redir = VerificarPermiso("EMPLEADOS_VER");
            if (redir != null) return redir;

            return View(_negocio.Listar());
        }

        // ----------------------------------------------------------------------------------
        // Crear
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Crear()
        {
            var redir = VerificarPermiso("EMPLEADOS_EDITAR");
            if (redir != null) return redir;

            var modelo = new EmpleadoCrearViewModel { Roles = _negocio.ListarRolesActivos() };
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(EmpleadoCrearViewModel modelo)
        {
            var redir = VerificarPermiso("EMPLEADOS_EDITAR");
            if (redir != null) return redir;

            try
            {
                _negocio.Crear(modelo.Empleado, modelo.Contrasena, UsuarioActual);
                TempData["Exito"] = "Empleado creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                modelo.Roles = _negocio.ListarRolesActivos();
                return View(modelo);
            }
        }

        // ----------------------------------------------------------------------------------
        // Editar (sin usuario ni contraseña)
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Editar(string id)
        {
            var redir = VerificarPermiso("EMPLEADOS_EDITAR");
            if (redir != null) return redir;

            Empleado? empleado = _negocio.Obtener(id);
            if (empleado == null)
            {
                TempData["Error"] = "El empleado indicado no existe.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = _negocio.ListarRolesActivos();
            return View(empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Empleado modelo)
        {
            var redir = VerificarPermiso("EMPLEADOS_EDITAR");
            if (redir != null) return redir;

            try
            {
                _negocio.Editar(modelo, UsuarioActual);
                TempData["Exito"] = "Empleado actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Roles = _negocio.ListarRolesActivos();
                return View(modelo);
            }
        }

        // ----------------------------------------------------------------------------------
        // Eliminar (lógico) — no puede desactivarse a sí mismo
        // ----------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(string id)
        {
            var redir = VerificarPermiso("EMPLEADOS_ELIMINAR");
            if (redir != null) return redir;

            // Cédula del usuario en sesión: la regla de negocio impide auto-desactivarse.
            string cedulaActual = HttpContext.Session.GetString(SesionWeb.ClaveCedula) ?? string.Empty;

            try
            {
                _negocio.Eliminar(id, cedulaActual, UsuarioActual);
                TempData["Exito"] = "Empleado desactivado correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
