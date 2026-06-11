using TallerJuan.Entidades;

namespace TallerJuan.Web.Models
{
    /// <summary>
    /// ViewModel para crear una orden de trabajo. Lleva los dropdowns de clientes y mecánicos;
    /// el dropdown de vehículos se carga por JavaScript según el cliente elegido.
    /// </summary>
    public class OrdenCrearViewModel
    {
        /// <summary>Datos de la orden a crear (vehículo, mecánico, kilometraje, motivo, problema).</summary>
        public OrdenTrabajo Orden { get; set; } = new OrdenTrabajo();

        /// <summary>Cédula del cliente seleccionado (para filtrar sus vehículos).</summary>
        public string ClienteCedula { get; set; } = string.Empty;

        /// <summary>Clientes ACTIVOS disponibles.</summary>
        public List<Cliente> Clientes { get; set; } = new List<Cliente>();

        /// <summary>Empleados ACTIVOS disponibles como mecánicos asignables.</summary>
        public List<Empleado> Mecanicos { get; set; } = new List<Empleado>();
    }
}
