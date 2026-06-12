using Microsoft.AspNetCore.Mvc;
using TallerJuan.Negocio;
using TallerJuan.Web.Models;
using TallerJuan.Web.Seguridad;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Controlador del dashboard de inicio que se muestra tras el login (Fase 7).
    /// Arma las tarjetas resumen mostrando solo las de los módulos a los que el usuario tiene
    /// permiso y calculando cada conteo en C# a partir de los métodos Listar ya existentes en la
    /// capa de Negocio (sin SPs nuevos).
    /// </summary>
    public class InicioController : Controller
    {
        private readonly CN_Cliente _clientes = new CN_Cliente();
        private readonly CN_OrdenTrabajo _ordenes = new CN_OrdenTrabajo();
        private readonly CN_Producto _productos = new CN_Producto();
        private readonly CN_Cotizacion _cotizaciones = new CN_Cotizacion();
        private readonly CN_Factura _facturas = new CN_Factura();

        public IActionResult Index()
        {
            var sesion = HttpContext.Session;

            var modelo = new DashboardViewModel
            {
                Nombre = SesionWeb.Nombre(sesion),
                Rol = SesionWeb.RolNombre(sesion),
                VerClientes = SesionWeb.TienePermiso(sesion, "CLIENTES_VER"),
                VerOrdenes = SesionWeb.TienePermiso(sesion, "ORDENES_VER"),
                VerInventario = SesionWeb.TienePermiso(sesion, "INVENTARIO_VER"),
                VerCotizaciones = SesionWeb.TienePermiso(sesion, "COTIZACIONES_VER"),
                VerFacturas = SesionWeb.TienePermiso(sesion, "FACTURAS_VER")
            };

            // Cada conteo se calcula solo si el usuario puede ver ese módulo.
            if (modelo.VerClientes)
                modelo.ClientesActivos = _clientes.Listar()
                    .Count(c => c.Estado.Trim().ToUpperInvariant() == "ACTIVO");

            if (modelo.VerOrdenes)
                modelo.OrdenesEnProceso = _ordenes.Listar()
                    .Count(o => !CN_OrdenTrabajo.EsEntregada(o.Estado));

            if (modelo.VerInventario)
                modelo.ProductosBajoStock = _productos.AlertasStockMinimo().Count;

            if (modelo.VerCotizaciones)
                modelo.CotizacionesPendientes = _cotizaciones.Listar()
                    .Count(c => CN_Cotizacion.EsEditable(c.Estado));

            if (modelo.VerFacturas)
            {
                DateTime hoy = DateTime.Today;
                modelo.FacturasDelMes = _facturas.Listar()
                    .Count(f => f.Estado.Trim().ToUpperInvariant() == "EMITIDA"
                                && f.FechaEmision.Year == hoy.Year
                                && f.FechaEmision.Month == hoy.Month);
            }

            return View(modelo);
        }
    }
}
