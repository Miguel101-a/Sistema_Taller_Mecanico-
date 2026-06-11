namespace TallerJuan.Entidades
{
    /// <summary>
    /// Representa a un cliente del taller (propietario de uno o varios vehículos).
    /// La cédula es el identificador principal.
    /// </summary>
    public class Cliente
    {
        /// <summary>Cédula del cliente (identificador principal).</summary>
        public string Cedula { get; set; } = string.Empty;

        /// <summary>Nombre completo o razón social del cliente.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Teléfono de contacto.</summary>
        public string Telefono { get; set; } = string.Empty;

        /// <summary>Dirección del cliente.</summary>
        public string Direccion { get; set; } = string.Empty;

        /// <summary>Correo electrónico de contacto.</summary>
        public string Correo { get; set; } = string.Empty;

        /// <summary>Tipo de cliente (particular / empresarial).</summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>Fecha de registro del cliente.</summary>
        public DateTime FechaRegistro { get; set; }

        /// <summary>Estado del cliente (ACTIVO / INACTIVO).</summary>
        public string Estado { get; set; } = string.Empty;
    }
}
