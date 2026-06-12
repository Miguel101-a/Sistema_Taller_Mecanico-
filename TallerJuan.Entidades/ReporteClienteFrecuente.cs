namespace TallerJuan.Entidades
{
    /// <summary>
    /// Fila del reporte de clientes frecuentes (RF-41). Resume las órdenes y el monto facturado
    /// (facturas EMITIDAS) por cliente en el período.
    /// </summary>
    public class ReporteClienteFrecuente
    {
        /// <summary>Cédula del cliente.</summary>
        public string Cedula { get; set; } = string.Empty;

        /// <summary>Nombre del cliente.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Teléfono del cliente.</summary>
        public string Telefono { get; set; } = string.Empty;

        /// <summary>Total de órdenes del cliente en el período.</summary>
        public int TotalOrdenes { get; set; }

        /// <summary>Monto total facturado (facturas EMITIDAS) del cliente en el período.</summary>
        public decimal MontoFacturado { get; set; }
    }
}
