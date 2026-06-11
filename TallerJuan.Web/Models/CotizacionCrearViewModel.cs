using TallerJuan.Entidades;

namespace TallerJuan.Web.Models
{
    /// <summary>
    /// ViewModel para crear la cabecera de una cotización. Lleva el dropdown de clientes; el de
    /// vehículos se carga por JavaScript según el cliente elegido (mismo mecanismo de las órdenes).
    /// </summary>
    public class CotizacionCrearViewModel
    {
        /// <summary>Cédula del cliente seleccionado.</summary>
        public string ClienteCedula { get; set; } = string.Empty;

        /// <summary>Placa del vehículo seleccionado (cargado en cascada).</summary>
        public string VehiculoPlaca { get; set; } = string.Empty;

        /// <summary>Validez de la cotización en días (por defecto 15).</summary>
        public int ValidezDias { get; set; } = 15;

        /// <summary>Clientes ACTIVOS disponibles.</summary>
        public List<Cliente> Clientes { get; set; } = new List<Cliente>();
    }
}
