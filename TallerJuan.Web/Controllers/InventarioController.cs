using Microsoft.AspNetCore.Mvc;
using TallerJuan.Entidades;
using TallerJuan.Negocio;
using TallerJuan.Web.Seguridad;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Inventario y Repuestos (Fase 4): CRUD de productos con alerta de stock mínimo.
    /// Los movimientos de inventario son de la Fase 5; aquí el stock solo se define al crear.
    /// Permisos por acción (VER / EDITAR) y auditoría.
    /// </summary>
    public class InventarioController : Controller
    {
        private readonly CN_Producto _negocio = new CN_Producto();

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
        // Listado (con aviso de stock mínimo)
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Index()
        {
            var redir = VerificarPermiso("INVENTARIO_VER");
            if (redir != null) return redir;

            // Productos en o por debajo del stock mínimo: se muestran como aviso ámbar arriba de la tabla.
            ViewBag.AlertasStock = _negocio.AlertasStockMinimo();
            return View(_negocio.Listar());
        }

        // ----------------------------------------------------------------------------------
        // Crear
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Crear()
        {
            var redir = VerificarPermiso("INVENTARIO_EDITAR");
            if (redir != null) return redir;

            return View(new Producto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Producto modelo)
        {
            var redir = VerificarPermiso("INVENTARIO_EDITAR");
            if (redir != null) return redir;

            try
            {
                _negocio.Crear(modelo, UsuarioActual);
                TempData["Exito"] = "Producto creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                return View(modelo);
            }
        }

        // ----------------------------------------------------------------------------------
        // Editar (sin tocar STOCK_ACTUAL)
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Editar(string id)
        {
            var redir = VerificarPermiso("INVENTARIO_EDITAR");
            if (redir != null) return redir;

            Producto? producto = _negocio.Obtener(id);
            if (producto == null)
            {
                TempData["Error"] = "El producto indicado no existe.";
                return RedirectToAction(nameof(Index));
            }

            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Producto modelo)
        {
            var redir = VerificarPermiso("INVENTARIO_EDITAR");
            if (redir != null) return redir;

            try
            {
                _negocio.Editar(modelo, UsuarioActual);
                TempData["Exito"] = "Producto actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                return View(modelo);
            }
        }

        // ----------------------------------------------------------------------------------
        // Eliminar (lógico) — usa el permiso INVENTARIO_EDITAR
        // ----------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(string id)
        {
            var redir = VerificarPermiso("INVENTARIO_EDITAR");
            if (redir != null) return redir;

            try
            {
                _negocio.Eliminar(id, UsuarioActual);
                TempData["Exito"] = "Producto desactivado correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
