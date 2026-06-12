using Microsoft.Data.SqlClient;
using TallerJuan.Datos;
using TallerJuan.Entidades;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Capa de Negocio para la cabecera de Facturas. Controla los estados
    /// (BORRADOR / EMITIDA / ANULADA), fuerza la relación 1:1 con ORDEN_TRABAJO (traduce la
    /// violación de la UNIQUE), exige líneas para emitir, precarga repuestos de la orden y registra
    /// el motivo de anulación en auditoría (la tabla FACTURA no tiene columna de motivo). El detalle
    /// (N:M) lo maneja CN_DetalleFactura.
    /// </summary>
    public class CN_Factura
    {
        private const string Modulo = "Facturacion";
        private const int ErrorUniqueIndex = 2601;
        private const int ErrorUniqueConstraint = 2627;

        public const string EstadoBorrador = "BORRADOR";
        public const string EstadoEmitida = "EMITIDA";
        public const string EstadoAnulada = "ANULADA";

        /// <summary>Estados de orden que admiten facturación.</summary>
        private static readonly string[] EstadosOrdenFacturable = { "FINALIZADO", "ENTREGADO" };

        private readonly CD_Factura _datos = new CD_Factura();
        private readonly CD_DetalleFactura _detalles = new CD_DetalleFactura();
        private readonly CD_OrdenTrabajo _ordenes = new CD_OrdenTrabajo();
        private readonly CD_Acceso _acceso = new CD_Acceso();

        /// <summary>Devuelve todas las facturas (más recientes primero).</summary>
        public List<Factura> Listar() => _datos.Listar();

        /// <summary>Devuelve una factura por su número, o null si no existe.</summary>
        public Factura? Obtener(int numeroFactura) => _datos.Obtener(numeroFactura);

        /// <summary>Devuelve las órdenes facturables (FINALIZADO/ENTREGADO y sin factura previa).</summary>
        public List<OrdenTrabajo> ListarOrdenesFacturables() => _datos.ListarOrdenesFacturables();

        /// <summary>
        /// Crea la factura en estado BORRADOR a partir de una orden FINALIZADO/ENTREGADO sin factura
        /// previa. Si la orden ya tiene factura (UNIQUE 1:1) traduce el error. Audita y devuelve el
        /// NUMERO_FACTURA generado.
        /// </summary>
        public int Crear(int numeroOrden, string usuarioAccion)
        {
            OrdenTrabajo orden = _ordenes.Obtener(numeroOrden)
                ?? throw new InvalidOperationException("La orden de trabajo indicada no existe.");

            if (!EstadosOrdenFacturable.Contains(orden.Estado.Trim().ToUpperInvariant()))
                throw new InvalidOperationException("Solo se pueden facturar órdenes FINALIZADO o ENTREGADO.");

            int numero;
            try
            {
                numero = _datos.Insertar(numeroOrden);
            }
            catch (SqlException ex) when (EsDuplicado(ex))
            {
                throw new InvalidOperationException("La orden de trabajo ya tiene una factura asociada.");
            }

            _acceso.RegistrarAuditoria(usuarioAccion, Modulo,
                $"Crear factura (borrador) Nº {numero} desde orden Nº {numeroOrden}");
            return numero;
        }

        /// <summary>
        /// Precarga como líneas TIPO = REPUESTO los repuestos que salieron del inventario para la
        /// orden de la factura (salidas netas), omitiendo silenciosamente los que ya están en la
        /// factura. Solo en BORRADOR. Recalcula los totales y audita.
        /// </summary>
        public int CargarRepuestos(int numeroFactura, string usuarioAccion)
        {
            Factura factura = ObtenerOExcepcion(numeroFactura);
            if (!EsEditable(factura.Estado))
                throw new InvalidOperationException("Solo se pueden cargar repuestos en una factura en BORRADOR.");

            var existentes = _detalles.ListarPorFactura(numeroFactura)
                .Select(d => d.ProductoCodigo.Trim().ToUpperInvariant())
                .ToHashSet();

            int agregados = 0;
            foreach (var repuesto in _datos.RepuestosDeOrden(factura.OrdenTrabajoNumeroOrden))
            {
                if (existentes.Contains(repuesto.ProductoCodigo.Trim().ToUpperInvariant()))
                    continue; // ya está en la factura: se omite silenciosamente

                repuesto.FacturaNumeroFactura = numeroFactura;
                _detalles.Insertar(repuesto);
                agregados++;
            }

            if (agregados > 0)
            {
                _datos.RecalcularTotales(numeroFactura);
                _acceso.RegistrarAuditoria(usuarioAccion, Modulo,
                    $"Cargar {agregados} repuesto(s) de la orden a la factura Nº {numeroFactura}");
            }

            return agregados;
        }

        /// <summary>
        /// Emite la factura (RF-33): solo desde BORRADOR y con al menos una línea. Pasa a EMITIDA
        /// (el SP fija la FECHA_EMISION) y audita.
        /// </summary>
        public void Emitir(int numeroFactura, string usuarioAccion)
        {
            Factura factura = ObtenerOExcepcion(numeroFactura);
            if (!EsEditable(factura.Estado))
                throw new InvalidOperationException("Solo se puede emitir una factura en BORRADOR.");

            if (_detalles.ListarPorFactura(numeroFactura).Count == 0)
                throw new InvalidOperationException("La factura debe tener al menos una línea.");

            _datos.CambiarEstado(numeroFactura, EstadoEmitida);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Emitir factura Nº {numeroFactura}");
        }

        /// <summary>
        /// Anula la factura (RF-34): solo desde EMITIDA y con motivo obligatorio. Pasa a ANULADA y
        /// registra el motivo en AUDITORIA (la tabla FACTURA no tiene columna de motivo).
        /// </summary>
        public void Anular(int numeroFactura, string motivo, string usuarioAccion)
        {
            motivo = (motivo ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(motivo))
                throw new InvalidOperationException("Debe indicar el motivo de la anulación.");

            Factura factura = ObtenerOExcepcion(numeroFactura);
            if (factura.Estado.Trim().ToUpperInvariant() != EstadoEmitida)
                throw new InvalidOperationException("Solo se puede anular una factura EMITIDA.");

            _datos.CambiarEstado(numeroFactura, EstadoAnulada);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo,
                $"Anular factura Nº {numeroFactura} — Motivo: {motivo}");
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Indica si la factura está en BORRADOR (admite cambios en sus líneas).</summary>
        public static bool EsEditable(string estado) =>
            (estado ?? string.Empty).Trim().ToUpperInvariant() == EstadoBorrador;

        /// <summary>Obtiene la factura o lanza una excepción de negocio si no existe.</summary>
        private Factura ObtenerOExcepcion(int numeroFactura) =>
            _datos.Obtener(numeroFactura)
            ?? throw new InvalidOperationException("La factura indicada no existe.");

        /// <summary>Detecta violación de clave única (orden con factura repetida -> 1:1).</summary>
        private static bool EsDuplicado(SqlException ex) =>
            ex.Number == ErrorUniqueConstraint || ex.Number == ErrorUniqueIndex;
    }
}
