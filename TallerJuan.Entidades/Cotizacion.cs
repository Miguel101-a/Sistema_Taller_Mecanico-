namespace TallerJuan.Entidades
{
    /// <summary>
    /// Cabecera de una cotización para un cliente y su vehículo. Los subtotales por
    /// tipo, el IVA (13%) y el total se recalculan automáticamente a partir de sus líneas
    /// (DETALLE_COTIZACION). El número es autonumérico y es el identificador principal.
    /// </summary>
    public class Cotizacion
    {
        /// <summary>Número de cotización (identificador principal, autonumérico).</summary>
        public int NumeroCotizacion { get; set; }

        /// <summary>Fecha de emisión de la cotización.</summary>
        public DateTime FechaEmision { get; set; }

        /// <summary>Validez de la cotización en días (opcional).</summary>
        public int? ValidezDias { get; set; }

        /// <summary>Subtotal de las líneas de tipo SERVICIO.</summary>
        public decimal SubtotalServicios { get; set; }

        /// <summary>Subtotal de las líneas de tipo REPUESTO.</summary>
        public decimal SubtotalRepuestos { get; set; }

        /// <summary>Impuestos (IVA 13% sobre la suma de subtotales).</summary>
        public decimal Impuestos { get; set; }

        /// <summary>Total de la cotización (servicios + repuestos + impuestos).</summary>
        public decimal Total { get; set; }

        /// <summary>Estado de la cotización: PENDIENTE / APROBADA / RECHAZADA.</summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Cédula del cliente (FK a CLIENTE).</summary>
        public string ClienteCedula { get; set; } = string.Empty;

        /// <summary>Nombre del cliente (solo para mostrar).</summary>
        public string ClienteNombre { get; set; } = string.Empty;

        /// <summary>Placa del vehículo (FK a VEHICULO).</summary>
        public string VehiculoPlaca { get; set; } = string.Empty;

        /// <summary>Descripción del vehículo "marca modelo" (solo para mostrar).</summary>
        public string VehiculoDescripcion { get; set; } = string.Empty;
    }
}
