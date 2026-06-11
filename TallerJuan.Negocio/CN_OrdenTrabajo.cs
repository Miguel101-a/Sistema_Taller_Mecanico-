using TallerJuan.Datos;
using TallerJuan.Entidades;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Capa de Negocio para las Órdenes de Trabajo (Fase 4). Controla los estados válidos,
    /// impide editar/cambiar una orden ya ENTREGADA, valida los datos y registra la auditoría.
    /// </summary>
    public class CN_OrdenTrabajo
    {
        private const string Modulo = "Ordenes";

        // Estado inicial al crear y estado final (no editable).
        public const string EstadoEntregado = "ENTREGADO";

        /// <summary>Lista ordenada de estados válidos del flujo de una orden de trabajo.</summary>
        public static readonly IReadOnlyList<string> EstadosValidos = new[]
        {
            "RECIBIDO", "EN DIAGNOSTICO", "EN REPARACION", "FINALIZADO", "ENTREGADO"
        };

        private readonly CD_OrdenTrabajo _datos = new CD_OrdenTrabajo();
        private readonly CD_Acceso _acceso = new CD_Acceso();

        /// <summary>Devuelve todas las órdenes de trabajo (más recientes primero).</summary>
        public List<OrdenTrabajo> Listar() => _datos.Listar();

        /// <summary>Devuelve una orden por su número, o null si no existe.</summary>
        public OrdenTrabajo? Obtener(int numeroOrden) => _datos.Obtener(numeroOrden);

        /// <summary>
        /// Crea una orden de trabajo. Valida vehículo, mecánico y kilometraje, registra la
        /// auditoría y devuelve el NUMERO_ORDEN generado. La orden nace en estado RECIBIDO.
        /// </summary>
        public int Crear(OrdenTrabajo orden, string usuarioAccion)
        {
            Normalizar(orden);

            if (string.IsNullOrWhiteSpace(orden.VehiculoPlaca))
                throw new InvalidOperationException("Debe seleccionar el vehículo de la orden.");
            if (string.IsNullOrWhiteSpace(orden.EmpleadoCedula))
                throw new InvalidOperationException("Debe seleccionar el mecánico asignado.");
            if (orden.KilometrajeIngreso.HasValue && orden.KilometrajeIngreso < 0)
                throw new InvalidOperationException("El kilometraje de ingreso no puede ser negativo.");

            int numero = _datos.Insertar(orden);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Crear orden de trabajo N° {numero}");
            return numero;
        }

        /// <summary>
        /// Edita los datos de una orden. No se permite editar una orden ya ENTREGADA.
        /// Valida y registra la auditoría.
        /// </summary>
        public void Editar(OrdenTrabajo orden, string usuarioAccion)
        {
            OrdenTrabajo actual = _datos.Obtener(orden.NumeroOrden)
                ?? throw new InvalidOperationException("La orden de trabajo indicada no existe.");

            if (EsEntregada(actual.Estado))
                throw new InvalidOperationException("La orden ya fue entregada y no se puede editar.");

            Normalizar(orden);

            if (string.IsNullOrWhiteSpace(orden.EmpleadoCedula))
                throw new InvalidOperationException("Debe seleccionar el mecánico asignado.");
            if (orden.KilometrajeIngreso.HasValue && orden.KilometrajeIngreso < 0)
                throw new InvalidOperationException("El kilometraje de ingreso no puede ser negativo.");

            _datos.Actualizar(orden);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Editar orden de trabajo N° {orden.NumeroOrden}");
        }

        /// <summary>
        /// Cambia el estado de una orden. Valida que el estado destino sea uno de los válidos y
        /// que la orden no esté ya ENTREGADA. Registra la auditoría.
        /// </summary>
        public void CambiarEstado(int numeroOrden, string nuevoEstado, string usuarioAccion)
        {
            string estado = (nuevoEstado ?? string.Empty).Trim().ToUpperInvariant();

            if (!EstadosValidos.Contains(estado))
                throw new InvalidOperationException("El estado indicado no es válido.");

            OrdenTrabajo actual = _datos.Obtener(numeroOrden)
                ?? throw new InvalidOperationException("La orden de trabajo indicada no existe.");

            if (EsEntregada(actual.Estado))
                throw new InvalidOperationException("La orden ya fue entregada y no puede cambiar de estado.");

            _datos.CambiarEstado(numeroOrden, estado);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Orden N° {numeroOrden}: estado -> {estado}");
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Indica si un estado corresponde a ENTREGADO (orden cerrada).</summary>
        public static bool EsEntregada(string estado) =>
            (estado ?? string.Empty).Trim().ToUpperInvariant() == EstadoEntregado;

        /// <summary>Recorta los espacios de los campos de texto editables de la orden.</summary>
        private static void Normalizar(OrdenTrabajo o)
        {
            o.MotivoVisita = (o.MotivoVisita ?? string.Empty).Trim();
            o.DescripcionProblema = (o.DescripcionProblema ?? string.Empty).Trim();
            o.VehiculoPlaca = (o.VehiculoPlaca ?? string.Empty).Trim().ToUpperInvariant();
            o.EmpleadoCedula = (o.EmpleadoCedula ?? string.Empty).Trim();
        }
    }
}
