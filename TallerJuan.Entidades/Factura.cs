namespace TallerJuan.Entidades
{
    /// <summary>
    /// Cabecera de una factura (Fase 6). Relación 1:1 con ORDEN_TRABAJO (forzada por la UNIQUE
    /// sobre la FK): una orden solo puede tener UNA factura. El subtotal, el IVA (13%) y el total
    /// se recalculan automáticamente a partir de sus líneas (DETALLE_FACTURA). El número es
    /// autonumérico, correlativo y único (RF-36) y es el identificador principal.
    /// </summary>
    public class Factura
    {
        /// <summary>Número de factura (identificador principal, autonumérico y correlativo).</summary>
        public int NumeroFactura { get; set; }

        /// <summary>Fecha de emisión (se fija al pasar a EMITIDA).</summary>
        public DateTime FechaEmision { get; set; }

        /// <summary>Subtotal de las líneas (sin IVA).</summary>
        public decimal Subtotal { get; set; }

        /// <summary>IVA (13% sobre el subtotal).</summary>
        public decimal Iva { get; set; }

        /// <summary>Total de la factura (subtotal + IVA).</summary>
        public decimal Total { get; set; }

        /// <summary>Estado de la factura: BORRADOR / EMITIDA / ANULADA.</summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Número de la orden de trabajo asociada (FK 1:1 con ORDEN_TRABAJO).</summary>
        public int OrdenTrabajoNumeroOrden { get; set; }

        /// <summary>Descripción del vehículo "marca modelo" (solo para mostrar).</summary>
        public string VehiculoDescripcion { get; set; } = string.Empty;

        /// <summary>Cédula del cliente (solo para mostrar).</summary>
        public string ClienteCedula { get; set; } = string.Empty;

        /// <summary>Nombre del cliente (solo para mostrar).</summary>
        public string ClienteNombre { get; set; } = string.Empty;
    }
}
