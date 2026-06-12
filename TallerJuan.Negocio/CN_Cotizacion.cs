using TallerJuan.Datos;
using TallerJuan.Entidades;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Capa de Negocio para la cabecera de Cotizaciones. Controla los estados
    /// (PENDIENTE / APROBADA / RECHAZADA), valida cliente, vehículo y validez, e impide cambiar
    /// una cotización ya cerrada. Registra la auditoría. El detalle (N:M) lo maneja
    /// CN_DetalleCotizacion.
    /// </summary>
    public class CN_Cotizacion
    {
        private const string Modulo = "Cotizaciones";

        public const string EstadoPendiente = "PENDIENTE";
        public const string EstadoAprobada = "APROBADA";
        public const string EstadoRechazada = "RECHAZADA";

        /// <summary>Estados a los que se puede cambiar una cotización PENDIENTE.</summary>
        private static readonly string[] EstadosDestino = { EstadoAprobada, EstadoRechazada };

        private readonly CD_Cotizacion _datos = new CD_Cotizacion();
        private readonly CD_Acceso _acceso = new CD_Acceso();

        /// <summary>Devuelve todas las cotizaciones (más recientes primero).</summary>
        public List<Cotizacion> Listar() => _datos.Listar();

        /// <summary>Devuelve una cotización por su número, o null si no existe.</summary>
        public Cotizacion? Obtener(int numeroCotizacion) => _datos.Obtener(numeroCotizacion);

        /// <summary>
        /// Crea una cotización (nace en estado PENDIENTE). Valida cliente, vehículo y la validez
        /// (entre 1 y 90 días). Registra la auditoría y devuelve el NUMERO_COTIZACION generado.
        /// </summary>
        public int Crear(Cotizacion cotizacion, string usuarioAccion)
        {
            cotizacion.ClienteCedula = (cotizacion.ClienteCedula ?? string.Empty).Trim();
            cotizacion.VehiculoPlaca = (cotizacion.VehiculoPlaca ?? string.Empty).Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(cotizacion.ClienteCedula))
                throw new InvalidOperationException("Debe seleccionar el cliente.");
            if (string.IsNullOrWhiteSpace(cotizacion.VehiculoPlaca))
                throw new InvalidOperationException("Debe seleccionar el vehículo.");
            if (!cotizacion.ValidezDias.HasValue || cotizacion.ValidezDias < 1 || cotizacion.ValidezDias > 90)
                throw new InvalidOperationException("La validez debe estar entre 1 y 90 días.");

            int numero = _datos.Insertar(cotizacion);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Crear cotización N° {numero}");
            return numero;
        }

        /// <summary>
        /// Cambia el estado de una cotización a APROBADA o RECHAZADA. Solo se permite si la
        /// cotización está PENDIENTE; una vez aprobada o rechazada ya no se modifica. Registra
        /// la auditoría.
        /// </summary>
        public void CambiarEstado(int numeroCotizacion, string nuevoEstado, string usuarioAccion)
        {
            string estado = (nuevoEstado ?? string.Empty).Trim().ToUpperInvariant();

            if (!EstadosDestino.Contains(estado))
                throw new InvalidOperationException("El estado indicado no es válido.");

            Cotizacion actual = _datos.Obtener(numeroCotizacion)
                ?? throw new InvalidOperationException("La cotización indicada no existe.");

            if (!EsEditable(actual.Estado))
                throw new InvalidOperationException("La cotización ya fue aprobada o rechazada; no admite más cambios.");

            _datos.CambiarEstado(numeroCotizacion, estado);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Cotización N° {numeroCotizacion}: estado -> {estado}");
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Indica si una cotización está PENDIENTE (puede editar líneas y cambiar de estado).</summary>
        public static bool EsEditable(string estado) =>
            (estado ?? string.Empty).Trim().ToUpperInvariant() == EstadoPendiente;
    }
}
