using Microsoft.AspNetCore.Mvc;
using TallerJuan.Entidades;
using TallerJuan.Negocio;
using TallerJuan.Web.Models;
using TallerJuan.Web.Seguridad;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Cotizaciones (Fase 5): cabecera COTIZACION + detalle N:M DETALLE_COTIZACION
    /// (COTIZACION ↔ PRODUCTO). Permite crear una cotización para un cliente/vehículo, agregar y
    /// quitar líneas (recalculando subtotales por tipo, IVA 13% y total) y aprobar/rechazar.
    /// Permisos por acción (VER / EDITAR) y auditoría.
    /// </summary>
    public class CotizacionesController : Controller
    {
        private readonly CN_Cotizacion _negocio = new CN_Cotizacion();
        private readonly CN_DetalleCotizacion _detalles = new CN_DetalleCotizacion();
        private readonly CN_Cliente _clientes = new CN_Cliente();
        private readonly CN_Vehiculo _vehiculos = new CN_Vehiculo();
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

        /// <summary>Clientes ACTIVOS, ordenados por nombre.</summary>
        private List<Cliente> ClientesActivos() =>
            _clientes.Listar()
                .Where(c => c.Estado.Trim().ToUpperInvariant() == "ACTIVO")
                .OrderBy(c => c.Nombre)
                .ToList();

        /// <summary>Productos ACTIVOS, ordenados por nombre (para agregar líneas).</summary>
        private List<Producto> ProductosActivos() =>
            _productos.Listar()
                .Where(p => p.Estado.Trim().ToUpperInvariant() == "ACTIVO")
                .OrderBy(p => p.Nombre)
                .ToList();

        // ----------------------------------------------------------------------------------
        // Listado
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Index()
        {
            var redir = VerificarPermiso("COTIZACIONES_VER");
            if (redir != null) return redir;

            return View(_negocio.Listar());
        }

        // ----------------------------------------------------------------------------------
        // Vehículos de un cliente (JSON, para el dropdown en cascada al crear cotización).
        // Mismo mecanismo que en Órdenes, pero gobernado por el permiso de cotizaciones.
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult VehiculosPorCliente(string cedula)
        {
            if (!SesionWeb.TienePermiso(HttpContext.Session, "COTIZACIONES_EDITAR"))
                return Json(new List<object>());

            var vehiculos = _vehiculos.ListarPorCliente(cedula ?? string.Empty)
                .Select(v => new
                {
                    placa = v.Placa,
                    descripcion = $"{v.Placa} — {v.Marca} {v.Modelo}".Trim()
                });

            return Json(vehiculos);
        }

        // ----------------------------------------------------------------------------------
        // Crear (cabecera)
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Crear()
        {
            var redir = VerificarPermiso("COTIZACIONES_EDITAR");
            if (redir != null) return redir;

            return View(new CotizacionCrearViewModel { Clientes = ClientesActivos() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(CotizacionCrearViewModel modelo)
        {
            var redir = VerificarPermiso("COTIZACIONES_EDITAR");
            if (redir != null) return redir;

            try
            {
                var cotizacion = new Cotizacion
                {
                    ClienteCedula = modelo.ClienteCedula,
                    VehiculoPlaca = modelo.VehiculoPlaca,
                    ValidezDias = modelo.ValidezDias
                };
                int numero = _negocio.Crear(cotizacion, UsuarioActual);
                TempData["Exito"] = $"Cotización N° {numero} creada correctamente.";
                return RedirectToAction(nameof(Detalle), new { numeroCotizacion = numero });
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                modelo.Clientes = ClientesActivos();
                return View(modelo);
            }
        }

        // ----------------------------------------------------------------------------------
        // Detalle (cabecera + líneas + totales + cambios de estado)
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Detalle(int numeroCotizacion)
        {
            var redir = VerificarPermiso("COTIZACIONES_VER");
            if (redir != null) return redir;

            Cotizacion? cotizacion = _negocio.Obtener(numeroCotizacion);
            if (cotizacion == null)
            {
                TempData["Error"] = "La cotización indicada no existe.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Detalles = _detalles.ListarPorCotizacion(numeroCotizacion);
            ViewBag.Productos = ProductosActivos();
            return View(cotizacion);
        }

        // ----------------------------------------------------------------------------------
        // Agregar línea (N:M COTIZACION ↔ PRODUCTO)
        // ----------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarLinea(DetalleCotizacion modelo)
        {
            var redir = VerificarPermiso("COTIZACIONES_EDITAR");
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

            return RedirectToAction(nameof(Detalle), new { numeroCotizacion = modelo.CotizacionNumeroCotizacion });
        }

        // ----------------------------------------------------------------------------------
        // Quitar línea
        // ----------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult QuitarLinea(int numeroCotizacion, string productoCodigo)
        {
            var redir = VerificarPermiso("COTIZACIONES_EDITAR");
            if (redir != null) return redir;

            try
            {
                _detalles.Quitar(numeroCotizacion, productoCodigo, UsuarioActual);
                TempData["Exito"] = "Línea quitada y totales recalculados.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detalle), new { numeroCotizacion });
        }

        // ----------------------------------------------------------------------------------
        // Cambiar estado (APROBAR / RECHAZAR)
        // ----------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstado(int numeroCotizacion, string estado)
        {
            var redir = VerificarPermiso("COTIZACIONES_EDITAR");
            if (redir != null) return redir;

            try
            {
                _negocio.CambiarEstado(numeroCotizacion, estado, UsuarioActual);
                TempData["Exito"] = $"Cotización N° {numeroCotizacion} actualizada a {estado.Trim().ToUpperInvariant()}.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detalle), new { numeroCotizacion });
        }
    }
}
