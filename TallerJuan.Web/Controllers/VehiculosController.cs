using Microsoft.AspNetCore.Mvc;
using TallerJuan.Entidades;
using TallerJuan.Negocio;
using TallerJuan.Web.Seguridad;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Vehículos (Fase 4): CRUD completo con eliminación lógica. Cada vehículo
    /// pertenece a un cliente (dropdown de clientes activos). Permisos por acción y auditoría.
    /// </summary>
    public class VehiculosController : Controller
    {
        private readonly CN_Vehiculo _negocio = new CN_Vehiculo();
        private readonly CN_Cliente _clientes = new CN_Cliente();

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

        /// <summary>Carga en el ViewBag la lista de clientes ACTIVOS para el dropdown del formulario.</summary>
        private void CargarClientesActivos()
        {
            ViewBag.Clientes = _clientes.Listar()
                .Where(c => c.Estado.Trim().ToUpperInvariant() == "ACTIVO")
                .OrderBy(c => c.Nombre)
                .ToList();
        }

        // ----------------------------------------------------------------------------------
        // Listado
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Index()
        {
            var redir = VerificarPermiso("VEHICULOS_VER");
            if (redir != null) return redir;

            return View(_negocio.Listar());
        }

        // ----------------------------------------------------------------------------------
        // Crear
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Crear()
        {
            var redir = VerificarPermiso("VEHICULOS_EDITAR");
            if (redir != null) return redir;

            CargarClientesActivos();
            return View(new Vehiculo());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Vehiculo vehiculo)
        {
            var redir = VerificarPermiso("VEHICULOS_EDITAR");
            if (redir != null) return redir;

            try
            {
                _negocio.Crear(vehiculo, UsuarioActual);
                TempData["Exito"] = "Vehículo creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                CargarClientesActivos();
                return View(vehiculo);
            }
        }

        // ----------------------------------------------------------------------------------
        // Editar
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Editar(string id)
        {
            var redir = VerificarPermiso("VEHICULOS_EDITAR");
            if (redir != null) return redir;

            Vehiculo? vehiculo = _negocio.Obtener(id);
            if (vehiculo == null)
            {
                TempData["Error"] = "El vehículo indicado no existe.";
                return RedirectToAction(nameof(Index));
            }

            CargarClientesActivos();
            return View(vehiculo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Vehiculo vehiculo)
        {
            var redir = VerificarPermiso("VEHICULOS_EDITAR");
            if (redir != null) return redir;

            try
            {
                _negocio.Editar(vehiculo, UsuarioActual);
                TempData["Exito"] = "Vehículo actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                CargarClientesActivos();
                return View(vehiculo);
            }
        }

        // ----------------------------------------------------------------------------------
        // Eliminar (lógico)
        // ----------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(string id)
        {
            var redir = VerificarPermiso("VEHICULOS_ELIMINAR");
            if (redir != null) return redir;

            try
            {
                _negocio.Eliminar(id, UsuarioActual);
                TempData["Exito"] = "Vehículo desactivado correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
