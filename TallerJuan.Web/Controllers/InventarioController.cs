using Microsoft.AspNetCore.Mvc;
using TallerJuan.Entidades;
using TallerJuan.Negocio;
using TallerJuan.Web.Models;
using TallerJuan.Web.Seguridad;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Inventario y Repuestos: CRUD de productos con alerta de stock mínimo (Fase 4) y
    /// movimientos de inventario (Fase 5, N:M PRODUCTO ↔ ORDEN_TRABAJO) que mueven el stock de forma
    /// transaccional. Permisos por acción (VER / EDITAR) y auditoría.
    /// </summary>
    public class InventarioController : Controller
    {
        private readonly CN_Producto _negocio = new CN_Producto();
        private readonly CN_MovimientoInventario _movimientos = new CN_MovimientoInventario();
        private readonly CN_OrdenTrabajo _ordenes = new CN_OrdenTrabajo();

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

        // ==================================================================================
        // FASE 5 — Movimientos de inventario (N:M PRODUCTO ↔ ORDEN_TRABAJO)
        // ==================================================================================

        /// <summary>Productos ACTIVOS, ordenados por nombre (para el dropdown de movimientos).</summary>
        private List<Producto> ProductosActivos() =>
            _negocio.Listar()
                .Where(p => p.Estado.Trim().ToUpperInvariant() == "ACTIVO")
                .OrderBy(p => p.Nombre)
                .ToList();

        /// <summary>Órdenes de trabajo no ENTREGADAS (las únicas que admiten movimientos).</summary>
        private List<OrdenTrabajo> OrdenesAbiertas() =>
            _ordenes.Listar()
                .Where(o => !CN_OrdenTrabajo.EsEntregada(o.Estado))
                .OrderByDescending(o => o.NumeroOrden)
                .ToList();

        // ----------------------------------------------------------------------------------
        // Listado de movimientos
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Movimientos()
        {
            var redir = VerificarPermiso("INVENTARIO_VER");
            if (redir != null) return redir;

            return View(_movimientos.Listar());
        }

        // ----------------------------------------------------------------------------------
        // Registrar movimiento (INGRESO / SALIDA)
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult RegistrarMovimiento()
        {
            var redir = VerificarPermiso("INVENTARIO_EDITAR");
            if (redir != null) return redir;

            var modelo = new MovimientoCrearViewModel
            {
                Productos = ProductosActivos(),
                Ordenes = OrdenesAbiertas()
            };
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarMovimiento(MovimientoCrearViewModel modelo)
        {
            var redir = VerificarPermiso("INVENTARIO_EDITAR");
            if (redir != null) return redir;

            try
            {
                _movimientos.Registrar(modelo.Movimiento, UsuarioActual);

                // Releemos el producto para informar el stock resultante en el mensaje de éxito.
                Producto? producto = _negocio.Obtener(modelo.Movimiento.ProductoCodigo);
                string stock = producto != null ? $" Stock actual: {producto.StockActual}." : string.Empty;
                TempData["Exito"] = $"Movimiento de {modelo.Movimiento.Tipo} registrado correctamente.{stock}";
                return RedirectToAction(nameof(Movimientos));
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                modelo.Productos = ProductosActivos();
                modelo.Ordenes = OrdenesAbiertas();
                return View(modelo);
            }
        }
    }
}
