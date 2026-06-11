using TallerJuan.Entidades;

namespace TallerJuan.Web.Models
{
    /// <summary>
    /// ViewModel para registrar un movimiento de inventario. Lleva los dropdowns de productos
    /// ACTIVOS y de órdenes de trabajo no ENTREGADAS; el stock de cada producto se muestra en el
    /// formulario vía atributos data-* en cada opción.
    /// </summary>
    public class MovimientoCrearViewModel
    {
        /// <summary>Datos del movimiento a registrar (producto, orden, tipo, cantidad, motivo).</summary>
        public MovimientoInventario Movimiento { get; set; } = new MovimientoInventario();

        /// <summary>Productos ACTIVOS disponibles para mover.</summary>
        public List<Producto> Productos { get; set; } = new List<Producto>();

        /// <summary>Órdenes de trabajo no ENTREGADAS a las que asociar el movimiento.</summary>
        public List<OrdenTrabajo> Ordenes { get; set; } = new List<OrdenTrabajo>();
    }
}
