using Microsoft.AspNetCore.Mvc;
using TallerJuan.Entidades;
using TallerJuan.Negocio;
using TallerJuan.Web.Models;
using TallerJuan.Web.Seguridad;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Seguridad (Roles y Permisos) — Fase 3.
    /// Panel del Administrador para gestionar la relación N:M ROL_PERMISO (asignar/quitar
    /// permisos por rol) y el CRUD básico de roles. Todas las acciones exigen el permiso
    /// ROLES_GESTIONAR; si el usuario no lo tiene, se redirige a Inicio con un mensaje.
    /// </summary>
    public class SeguridadController : Controller
    {
        private const string PermisoRequerido = "ROLES_GESTIONAR";

        private readonly CN_Seguridad _negocio = new CN_Seguridad();

        /// <summary>
        /// Verifica que el usuario en sesión tenga el permiso ROLES_GESTIONAR.
        /// Devuelve null si está autorizado, o un RedirectToAction a Inicio si no lo está.
        /// </summary>
        private IActionResult? VerificarAcceso()
        {
            if (!SesionWeb.TienePermiso(HttpContext.Session, PermisoRequerido))
            {
                TempData["Error"] = "No tiene permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Inicio");
            }
            return null;
        }

        /// <summary>Usuario en sesión, usado para registrar la auditoría de cada acción.</summary>
        private string UsuarioActual => SesionWeb.Usuario(HttpContext.Session);

        // ----------------------------------------------------------------------------------
        // Listado de roles
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// Lista TODOS los roles (activos e inactivos) con sus acciones.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            var redir = VerificarAcceso();
            if (redir != null) return redir;

            List<Rol> roles = _negocio.ListarTodosLosRoles();
            return View(roles);
        }

        // ----------------------------------------------------------------------------------
        // Gestión de permisos de un rol (N:M ROL_PERMISO)
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// Muestra todos los permisos del sistema agrupados por módulo, marcando los que el rol
        /// ya tiene asignados.
        /// </summary>
        [HttpGet]
        public IActionResult Permisos(int idRol)
        {
            var redir = VerificarAcceso();
            if (redir != null) return redir;

            Rol? rol = _negocio.ListarTodosLosRoles().FirstOrDefault(r => r.IdRol == idRol);
            if (rol == null)
            {
                TempData["Error"] = "El rol indicado no existe.";
                return RedirectToAction(nameof(Index));
            }

            // Catálogo completo de permisos y los que el rol ya posee (para marcar los checkboxes).
            List<Permiso> todos = _negocio.ListarTodosLosPermisos();
            var idsAsignados = _negocio.ObtenerPermisosPorRol(idRol)
                                       .Select(p => p.IdPermiso)
                                       .ToHashSet();

            var modelo = new PermisosRolViewModel
            {
                IdRol = rol.IdRol,
                NombreRol = rol.Nombre,
                Permisos = todos
                    .OrderBy(p => p.Modulo).ThenBy(p => p.Descripcion)
                    .Select(p => new PermisoMarcadoViewModel
                    {
                        IdPermiso = p.IdPermiso,
                        Clave = p.Clave,
                        Descripcion = p.Descripcion,
                        Modulo = p.Modulo,
                        Asignado = idsAsignados.Contains(p.IdPermiso)
                    })
                    .ToList()
            };

            return View(modelo);
        }

        /// <summary>
        /// Guarda los permisos marcados: calcula la diferencia contra los actuales y solo
        /// asigna o quita los cambios (demostración clara del manejo de la tabla N:M).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarPermisos(int idRol, List<int>? permisosSeleccionados)
        {
            var redir = VerificarAcceso();
            if (redir != null) return redir;

            var seleccionados = (permisosSeleccionados ?? new List<int>()).ToHashSet();
            var actuales = _negocio.ObtenerPermisosPorRol(idRol)
                                   .Select(p => p.IdPermiso)
                                   .ToHashSet();

            // A asignar: marcados que aún no estaban. A quitar: estaban pero ya no están marcados.
            var aAsignar = seleccionados.Except(actuales).ToList();
            var aQuitar = actuales.Except(seleccionados).ToList();

            try
            {
                foreach (int idPermiso in aAsignar)
                    _negocio.AsignarPermiso(idRol, idPermiso, UsuarioActual);

                foreach (int idPermiso in aQuitar)
                    _negocio.QuitarPermiso(idRol, idPermiso, UsuarioActual);

                TempData["Exito"] = "Permisos actualizados correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                // Regla de negocio violada (ej.: quitar ROLES_GESTIONAR al Administrador).
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Permisos), new { idRol });
        }

        // ----------------------------------------------------------------------------------
        // CRUD de roles
        // ----------------------------------------------------------------------------------

        /// <summary>Formulario para crear un rol nuevo.</summary>
        [HttpGet]
        public IActionResult CrearRol()
        {
            var redir = VerificarAcceso();
            if (redir != null) return redir;

            return View(new Rol());
        }

        /// <summary>Procesa la creación de un rol; ante nombre duplicado vuelve al formulario.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearRol(Rol modelo)
        {
            var redir = VerificarAcceso();
            if (redir != null) return redir;

            try
            {
                _negocio.CrearRol(modelo.Nombre, UsuarioActual);
                TempData["Exito"] = "Rol creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                // Nombre vacío o duplicado: se muestra el mensaje en el propio formulario.
                ViewBag.Error = ex.Message;
                return View(modelo);
            }
        }

        /// <summary>Formulario para renombrar un rol existente.</summary>
        [HttpGet]
        public IActionResult EditarRol(int idRol)
        {
            var redir = VerificarAcceso();
            if (redir != null) return redir;

            Rol? rol = _negocio.ListarTodosLosRoles().FirstOrDefault(r => r.IdRol == idRol);
            if (rol == null)
            {
                TempData["Error"] = "El rol indicado no existe.";
                return RedirectToAction(nameof(Index));
            }

            return View(rol);
        }

        /// <summary>Procesa el renombrado de un rol; ante nombre duplicado vuelve al formulario.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarRol(Rol modelo)
        {
            var redir = VerificarAcceso();
            if (redir != null) return redir;

            try
            {
                _negocio.EditarRol(modelo.IdRol, modelo.Nombre, UsuarioActual);
                TempData["Exito"] = "Rol actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                return View(modelo);
            }
        }

        /// <summary>
        /// Alterna el estado del rol entre ACTIVO e INACTIVO, respetando la regla del Administrador.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstado(int idRol)
        {
            var redir = VerificarAcceso();
            if (redir != null) return redir;

            Rol? rol = _negocio.ListarTodosLosRoles().FirstOrDefault(r => r.IdRol == idRol);
            if (rol == null)
            {
                TempData["Error"] = "El rol indicado no existe.";
                return RedirectToAction(nameof(Index));
            }

            // Estado destino: el contrario al actual.
            string nuevoEstado = rol.Estado.Trim().ToUpperInvariant() == "ACTIVO" ? "INACTIVO" : "ACTIVO";

            try
            {
                _negocio.CambiarEstadoRol(idRol, nuevoEstado, UsuarioActual);
                TempData["Exito"] = $"Rol {(nuevoEstado == "ACTIVO" ? "activado" : "desactivado")} correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
