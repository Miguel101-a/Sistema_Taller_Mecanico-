namespace TallerJuan.Web.Models
{
    /// <summary>
    /// Modelo del dashboard de inicio (Fase 7). Reúne el saludo al usuario y los conteos de las
    /// tarjetas resumen. Cada tarjeta tiene un indicador de visibilidad (según el permiso del
    /// módulo) y su valor; el controlador solo calcula los conteos de los módulos permitidos. Los
    /// valores se obtienen reutilizando los métodos Listar de la capa de Negocio (sin SPs nuevos).
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>Nombre completo del usuario en sesión (para el saludo).</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Nombre del rol del usuario en sesión (para el saludo).</summary>
        public string Rol { get; set; } = string.Empty;

        // ---- Clientes activos (CLIENTES_VER) ----
        public bool VerClientes { get; set; }
        public int ClientesActivos { get; set; }

        // ---- Órdenes en proceso, distintas de ENTREGADO (ORDENES_VER) ----
        public bool VerOrdenes { get; set; }
        public int OrdenesEnProceso { get; set; }

        // ---- Productos en o bajo el stock mínimo (INVENTARIO_VER) ----
        public bool VerInventario { get; set; }
        public int ProductosBajoStock { get; set; }

        // ---- Cotizaciones pendientes (COTIZACIONES_VER) ----
        public bool VerCotizaciones { get; set; }
        public int CotizacionesPendientes { get; set; }

        // ---- Facturas emitidas del mes en curso (FACTURAS_VER) ----
        public bool VerFacturas { get; set; }
        public int FacturasDelMes { get; set; }

        /// <summary>Indica si hay al menos una tarjeta visible (según los permisos del usuario).</summary>
        public bool HayTarjetas =>
            VerClientes || VerOrdenes || VerInventario || VerCotizaciones || VerFacturas;
    }
}
