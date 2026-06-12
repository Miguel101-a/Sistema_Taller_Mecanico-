using Microsoft.AspNetCore.Mvc;
using TallerJuan.Entidades;
using TallerJuan.Negocio;
using TallerJuan.Web.Models;
using TallerJuan.Web.Seguridad;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Facturación (Fase 6): cabecera FACTURA (1:1 con ORDEN_TRABAJO, forzada por la
    /// UNIQUE sobre la FK) + detalle N:M DETALLE_FACTURA (FACTURA ↔ PRODUCTO, la cuarta y última
    /// N:M del proyecto). Emite facturas desde órdenes finalizadas con IVA 13% automático (RF-33),
    /// las anula registrando el motivo en auditoría (RF-34) y las consulta (RF-35). Permisos por
    /// acción (VER / EMITIR / ANULAR) y auditoría.
    /// </summary>
    public class FacturacionController : Controller
    {
        private readonly CN_Factura _negocio = new CN_Factura();
        private readonly CN_DetalleFactura _detalles = new CN_DetalleFactura();
        private readonly CN_Producto _productos = new CN_Producto();

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

        /// <summary>Productos ACTIVOS, ordenados por nombre (para agregar líneas).</summary>
        private List<Producto> ProductosActivos() =>
            _productos.Listar()
                .Where(p => p.Estado.Trim().ToUpperInvariant() == "ACTIVO")
                .OrderBy(p => p.Nombre)
                .ToList();

        // ----------------------------------------------------------------------------------
        // Listado (RF-35)
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Index()
        {
            var redir = VerificarPermiso("FACTURAS_VER");
            if (redir != null) return redir;

            return View(_negocio.Listar());
        }

        // ----------------------------------------------------------------------------------
        // Crear (cabecera en BORRADOR, desde una orden facturable)
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Crear()
        {
            var redir = VerificarPermiso("FACTURAS_EMITIR");
            if (redir != null) return redir;

            return View(new FacturaCrearViewModel { OrdenesFacturables = _negocio.ListarOrdenesFacturables() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(FacturaCrearViewModel modelo)
        {
            var redir = VerificarPermiso("FACTURAS_EMITIR");
            if (redir != null) return redir;

            try
            {
                int numero = _negocio.Crear(modelo.NumeroOrden, UsuarioActual);
                TempData["Exito"] = $"Factura Nº {numero} creada en borrador. Agregue las líneas y emítala.";
                return RedirectToAction(nameof(Detalle), new { numeroFactura = numero });
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                modelo.OrdenesFacturables = _negocio.ListarOrdenesFacturables();
                return View(modelo);
            }
        }

        // ----------------------------------------------------------------------------------
        // Detalle (cabecera + líneas N:M + totales + emitir/anular)
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Detalle(int numeroFactura)
        {
            var redir = VerificarPermiso("FACTURAS_VER");
            if (redir != null) return redir;

            Factura? factura = _negocio.Obtener(numeroFactura);
            if (factura == null)
            {
                TempData["Error"] = "La factura indicada no existe.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Detalles = _detalles.ListarPorFactura(numeroFactura);
            ViewBag.Productos = ProductosActivos();
            return View(factura);
        }

        // ----------------------------------------------------------------------------------
        // Cargar repuestos de la orden (precarga desde inventario)
        // ----------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CargarRepuestos(int numeroFactura)
        {
            var redir = VerificarPermiso("FACTURAS_EMITIR");
            if (redir != null) return redir;

            try
            {
                int agregados = _negocio.CargarRepuestos(numeroFactura, UsuarioActual);
                TempData["Exito"] = agregados > 0
                    ? $"Se precargaron {agregados} repuesto(s) de la orden y se recalcularon los totales."
                    : "No había repuestos nuevos de la orden para precargar.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detalle), new { numeroFactura });
        }

        // ----------------------------------------------------------------------------------
        // Agregar línea (N:M FACTURA ↔ PRODUCTO)
        // ----------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarLinea(DetalleFactura modelo)
        {
            var redir = VerificarPermiso("FACTURAS_EMITIR");
            if (redir != null) return redir;

            try
            {
                _detalles.Agregar(modelo, UsuarioActual);
                TempData["Exito"] = "Línea agregada y totales recalculados.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detalle), new { numeroFactura = modelo.FacturaNumeroFactura });
        }

        // ----------------------------------------------------------------------------------
        // Quitar línea
        // ----------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult QuitarLinea(int numeroFactura, string productoCodigo)
        {
            var redir = VerificarPermiso("FACTURAS_EMITIR");
            if (redir != null) return redir;

            try
            {
                _detalles.Quitar(numeroFactura, productoCodigo, UsuarioActual);
                TempData["Exito"] = "Línea quitada y totales recalculados.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detalle), new { numeroFactura });
        }

        // ----------------------------------------------------------------------------------
        // Emitir factura (RF-33)
        // ----------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Emitir(int numeroFactura)
        {
            var redir = VerificarPermiso("FACTURAS_EMITIR");
            if (redir != null) return redir;

            try
            {
                _negocio.Emitir(numeroFactura, UsuarioActual);
                TempData["Exito"] = $"Factura Nº {numeroFactura} emitida correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detalle), new { numeroFactura });
        }

        // ----------------------------------------------------------------------------------
        // Anular factura (RF-34, motivo obligatorio -> auditoría)
        // ----------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Anular(int numeroFactura, string motivo)
        {
            var redir = VerificarPermiso("FACTURAS_ANULAR");
            if (redir != null) return redir;

            try
            {
                _negocio.Anular(numeroFactura, motivo, UsuarioActual);
                TempData["Exito"] = $"Factura Nº {numeroFactura} anulada. El motivo quedó registrado en auditoría.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detalle), new { numeroFactura });
        }
    }
}
