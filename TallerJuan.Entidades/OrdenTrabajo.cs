namespace TallerJuan.Entidades
{
    /// <summary>
    /// Representa una orden de trabajo abierta sobre un vehículo y asignada a un mecánico.
    /// El número de orden es autoincremental (IDENTITY).
    /// </summary>
    public class OrdenTrabajo
    {
        /// <summary>Número de orden (identificador autoincremental).</summary>
        public int NumeroOrden { get; set; }

        /// <summary>Fecha y hora de ingreso del vehículo.</summary>
        public DateTime FechaIngreso { get; set; }

        /// <summary>Kilometraje del vehículo al ingresar (puede ser nulo).</summary>
        public int? KilometrajeIngreso { get; set; }

        /// <summary>Motivo de la visita.</summary>
        public string MotivoVisita { get; set; } = string.Empty;

        /// <summary>Descripción del problema reportado.</summary>
        public string DescripcionProblema { get; set; } = string.Empty;

        /// <summary>Estado de la orden (RECIBIDO, EN DIAGNOSTICO, EN REPARACION, FINALIZADO, ENTREGADO).</summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Fecha de entrega del vehículo (puede ser nula hasta que se entrega).</summary>
        public DateTime? FechaEntrega { get; set; }

        /// <summary>Placa del vehículo asociado (clave foránea).</summary>
        public string VehiculoPlaca { get; set; } = string.Empty;

        /// <summary>Descripción del vehículo (marca + modelo) para mostrar.</summary>
        public string VehiculoDescripcion { get; set; } = string.Empty;

        /// <summary>Cédula del cliente propietario del vehículo (para mostrar).</summary>
        public string ClienteCedula { get; set; } = string.Empty;

        /// <summary>Nombre del cliente propietario del vehículo (para mostrar).</summary>
        public string ClienteNombre { get; set; } = string.Empty;

        /// <summary>Cédula del mecánico asignado (clave foránea).</summary>
        public string EmpleadoCedula { get; set; } = string.Empty;

        /// <summary>Nombre del mecánico asignado (para mostrar).</summary>
        public string EmpleadoNombre { get; set; } = string.Empty;
    }
}
