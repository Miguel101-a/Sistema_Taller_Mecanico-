using TallerJuan.Entidades;

namespace TallerJuan.Web.Models
{
    /// <summary>
    /// ViewModel para crear una factura. Lleva el dropdown de órdenes facturables
    /// (FINALIZADO/ENTREGADO y sin factura previa) y el número de orden elegido.
    /// </summary>
    public class FacturaCrearViewModel
    {
        /// <summary>Número de la orden de trabajo seleccionada para facturar.</summary>
        public int NumeroOrden { get; set; }

        /// <summary>Órdenes facturables disponibles.</summary>
        public List<OrdenTrabajo> OrdenesFacturables { get; set; } = new List<OrdenTrabajo>();
    }
}
