using Microsoft.AspNetCore.Mvc;
using TallerJuan.Entidades;
using TallerJuan.Negocio;
using TallerJuan.Web.Models;
using TallerJuan.Web.Seguridad;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Módulo de Órdenes de Trabajo (Fase 4): crear, listar, ver detalle, editar (si no está
    /// entregada), cambiar de estado y registrar diagnósticos. El flujo de creación encadena
    /// Cliente -> Vehículo (por JS) -> Mecánico. Permisos por acción y auditoría.
    /// </summary>
    public class OrdenesController : Controller
    {
        // Estados cuyo cambio requiere el permiso especial ORDENES_CERRAR.
        private static readonly string[] EstadosDeCierre = { "FINALIZADO", "ENTREGADO" };

        private readonly CN_OrdenTrabajo _negocio = new CN_OrdenTrabajo();
        private readonly CN_Diagnostico _diagnosticos = new CN_Diagnostico();
        private readonly CN_Cliente _clientes = new CN_Cliente();
        private readonly CN_Vehiculo _vehiculos = new CN_Vehiculo();
        private readonly CN_Empleado _empleados = new CN_Empleado();

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

        /// <summary>
        /// Mecánicos asignables: empleados ACTIVOS cuyo cargo contenga "Mecanico";
        /// si no hay coincidencias, devuelve todos los empleados activos.
        /// </summary>
        private List<Empleado> MecanicosAsignables()
        {
            var activos = _empleados.Listar()
                .Where(e => e.Estado.Trim().ToUpperInvariant() == "ACTIVO")
                .OrderBy(e => e.Nombre)
                .ToList();

            var mecanicos = activos
                .Where(e => e.Cargo.ToUpperInvariant().Contains("MECANIC"))
                .ToList();

            return mecanicos.Count > 0 ? mecanicos : activos;
        }

        // ----------------------------------------------------------------------------------
        // Listado
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Index()
        {
            var redir = VerificarPermiso("ORDENES_VER");
            if (redir != null) return redir;

            return View(_negocio.Listar());
        }

        // ----------------------------------------------------------------------------------
        // Vehículos de un cliente (JSON, para el dropdown en cascada al crear orden)
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult VehiculosPorCliente(string cedula)
        {
            if (!SesionWeb.TienePermiso(HttpContext.Session, "ORDENES_EDITAR"))
                return Json(new List<object>());

            var vehiculos = _vehiculos.ListarPorCliente(cedula ?? string.Empty)
                .Select(v => new
                {
                    placa = v.Placa,
                    descripcion = $"{v.Placa} — {v.Marca} {v.Modelo}".Trim(),
                    kilometraje = v.Kilometraje
                });

            return Json(vehiculos);
        }

        // ----------------------------------------------------------------------------------
        // Crear
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Crear()
        {
            var redir = VerificarPermiso("ORDENES_EDITAR");
            if (redir != null) return redir;

            var modelo = new OrdenCrearViewModel
            {
                Clientes = ClientesActivos(),
                Mecanicos = MecanicosAsignables()
            };
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(OrdenCrearViewModel modelo)
        {
            var redir = VerificarPermiso("ORDENES_EDITAR");
            if (redir != null) return redir;

            try
            {
                int numero = _negocio.Crear(modelo.Orden, UsuarioActual);
                TempData["Exito"] = $"Orden de trabajo N° {numero} creada correctamente.";
                return RedirectToAction(nameof(Detalle), new { id = numero });
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                modelo.Clientes = ClientesActivos();
                modelo.Mecanicos = MecanicosAsignables();
                return View(modelo);
            }
        }

        // ----------------------------------------------------------------------------------
        // Detalle (orden + diagnósticos + formulario + cambios de estado)
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Detalle(int id)
        {
            var redir = VerificarPermiso("ORDENES_VER");
            if (redir != null) return redir;

            OrdenTrabajo? orden = _negocio.Obtener(id);
            if (orden == null)
            {
                TempData["Error"] = "La orden de trabajo indicada no existe.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Diagnosticos = _diagnosticos.ListarPorOrden(id);
            return View(orden);
        }

        // ----------------------------------------------------------------------------------
        // Editar (solo si no está ENTREGADA)
        // ----------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var redir = VerificarPermiso("ORDENES_EDITAR");
            if (redir != null) return redir;

            OrdenTrabajo? orden = _negocio.Obtener(id);
            if (orden == null)
            {
                TempData["Error"] = "La orden de trabajo indicada no existe.";
                return RedirectToAction(nameof(Index));
            }

            if (CN_OrdenTrabajo.EsEntregada(orden.Estado))
            {
                TempData["Error"] = "La orden ya fue entregada y no se puede editar.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            ViewBag.Mecanicos = MecanicosAsignables();
            return View(orden);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(OrdenTrabajo modelo)
        {
            var redir = VerificarPermiso("ORDENES_EDITAR");
            if (redir != null) return redir;

            try
            {
                _negocio.Editar(modelo, UsuarioActual);
                TempData["Exito"] = "Orden de trabajo actualizada correctamente.";
                return RedirectToAction(nameof(Detalle), new { id = modelo.NumeroOrden });
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Mecanicos = MecanicosAsignables();
                return View(modelo);
            }
        }

        // ----------------------------------------------------------------------------------
        // Cambiar estado (FINALIZADO/ENTREGADO exigen ORDENES_CERRAR)
        // ----------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstado(int id, string estado)
        {
            // El paso a FINALIZADO o ENTREGADO requiere el permiso de cierre; el resto, edición.
            string estadoNormalizado = (estado ?? string.Empty).Trim().ToUpperInvariant();
            string permisoRequerido = EstadosDeCierre.Contains(estadoNormalizado)
                ? "ORDENES_CERRAR"
                : "ORDENES_EDITAR";

            var redir = VerificarPermiso(permisoRequerido);
            if (redir != null) return redir;

            try
            {
                _negocio.CambiarEstado(id, estadoNormalizado, UsuarioActual);
                TempData["Exito"] = $"Estado de la orden actualizado a {estadoNormalizado}.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detalle), new { id });
        }

        // ----------------------------------------------------------------------------------
        // Agregar diagnóstico (DIAGNOSTICO_EDITAR)
        // ----------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarDiagnostico(Diagnostico modelo)
        {
            var redir = VerificarPermiso("DIAGNOSTICO_EDITAR");
            if (redir != null) return redir;

            try
            {
                _diagnosticos.Agregar(modelo, UsuarioActual);
                TempData["Exito"] = "Diagnóstico agregado correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detalle), new { id = modelo.OrdenTrabajoNumeroOrden });
        }
    }
}
