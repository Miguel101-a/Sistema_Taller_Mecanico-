namespace TallerJuan.Entidades
{
    /// <summary>
    /// Representa a un vehículo registrado en el taller. Cada vehículo pertenece a un cliente.
    /// La placa es el identificador principal.
    /// </summary>
    public class Vehiculo
    {
        /// <summary>Placa del vehículo (identificador principal).</summary>
        public string Placa { get; set; } = string.Empty;

        /// <summary>Marca del vehículo.</summary>
        public string Marca { get; set; } = string.Empty;

        /// <summary>Modelo del vehículo.</summary>
        public string Modelo { get; set; } = string.Empty;

        /// <summary>Año de fabricación (puede ser nulo).</summary>
        public int? Anio { get; set; }

        /// <summary>Color del vehículo.</summary>
        public string Color { get; set; } = string.Empty;

        /// <summary>Número de chasis.</summary>
        public string NumeroChasis { get; set; } = string.Empty;

        /// <summary>Número de motor.</summary>
        public string NumeroMotor { get; set; } = string.Empty;

        /// <summary>Tipo de combustible (gasolina, diésel, etc.).</summary>
        public string TipoCombustible { get; set; } = string.Empty;

        /// <summary>Kilometraje actual del vehículo (puede ser nulo).</summary>
        public int? Kilometraje { get; set; }

        /// <summary>Cédula del cliente propietario (clave foránea).</summary>
        public string ClienteCedula { get; set; } = string.Empty;

        /// <summary>Nombre del cliente propietario (solo para mostrar en listados/detalle).</summary>
        public string ClienteNombre { get; set; } = string.Empty;

        /// <summary>Estado del vehículo (ACTIVO / INACTIVO).</summary>
        public string Estado { get; set; } = string.Empty;
    }
}
