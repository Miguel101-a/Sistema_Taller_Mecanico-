using Microsoft.AspNetCore.Mvc;
using TallerJuan.Entidades;
using TallerJuan.Negocio;
using TallerJuan.Web.Seguridad;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Clientes (Fase 4): CRUD completo con eliminación lógica.
    /// Cada acción exige el permiso correspondiente (VER / EDITAR / ELIMINAR); sin permiso
    /// se redirige a Inicio con un mensaje. Toda escritura registra auditoría en la capa de Negocio.
    /// </summary>
    public class ClientesController : Controller
    {
        private readonly CN_Cliente _negocio = new CN_Cliente();

        /// <summary>Verifica un permiso; devuelve null si está autorizado o un redirect a Inicio si no.</summary>
        private IActionResult? VerificarPermiso(string clave)
        {
            if (!SesionWeb.TienePermiso(HttpContext.Session, clave))
            {
                TempData["Error"] = "No tiene permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Inicio");
            }
            return null;
        }

        /// <summary>Usuario en sesión, para registrar la auditoría.</summary>
        private string UsuarioActual => SesionWeb.Usuario(HttpContext.Session);

        // ----------------------------------------------------------------------------------
        // Listado
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Index()
        {
            var redir = VerificarPermiso("CLIENTES_VER");
            if (redir != null) return redir;

            return View(_negocio.Listar());
        }

        // ----------------------------------------------------------------------------------
        // Crear
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Crear()
        {
            var redir = VerificarPermiso("CLIENTES_EDITAR");
            if (redir != null) return redir;

            return View(new Cliente());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Cliente modelo)
        {
            var redir = VerificarPermiso("CLIENTES_EDITAR");
            if (redir != null) return redir;

            try
            {
                _negocio.Crear(modelo, UsuarioActual);
                TempData["Exito"] = "Cliente creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                return View(modelo);
            }
        }

        // ----------------------------------------------------------------------------------
        // Editar
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Editar(string id)
        {
            var redir = VerificarPermiso("CLIENTES_EDITAR");
            if (redir != null) return redir;

            Cliente? cliente = _negocio.Obtener(id);
            if (cliente == null)
            {
                TempData["Error"] = "El cliente indicado no existe.";
                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Cliente modelo)
        {
            var redir = VerificarPermiso("CLIENTES_EDITAR");
            if (redir != null) return redir;

            try
            {
                _negocio.Editar(modelo, UsuarioActual);
                TempData["Exito"] = "Cliente actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                return View(modelo);
            }
        }

        // ----------------------------------------------------------------------------------
        // Eliminar (lógico)
        // ----------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(string id)
        {
            var redir = VerificarPermiso("CLIENTES_ELIMINAR");
            if (redir != null) return redir;

            try
            {
                _negocio.Eliminar(id, UsuarioActual);
                TempData["Exito"] = "Cliente desactivado correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
