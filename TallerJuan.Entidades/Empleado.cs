namespace TallerJuan.Entidades
{
    /// <summary>
    /// Representa a un empleado del taller. Es también el usuario del sistema.
    /// Por seguridad, la contraseña (hash) NO se expone como propiedad legible.
    /// </summary>
    public class Empleado
    {
        /// <summary>Cédula del empleado (identificador principal).</summary>
        public string Cedula { get; set; } = string.Empty;

        /// <summary>Nombre completo del empleado.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Teléfono de contacto.</summary>
        public string Telefono { get; set; } = string.Empty;

        /// <summary>Dirección de residencia.</summary>
        public string Direccion { get; set; } = string.Empty;

        /// <summary>Cargo que ocupa dentro del taller.</summary>
        public string Cargo { get; set; } = string.Empty;

        /// <summary>Fecha de ingreso al taller.</summary>
        public DateTime FechaIngreso { get; set; }

        /// <summary>Salario del empleado (puede ser nulo).</summary>
        public decimal? Salario { get; set; }

        /// <summary>Especialidad técnica del empleado.</summary>
        public string Especialidad { get; set; } = string.Empty;

        /// <summary>Nombre de usuario para iniciar sesión.</summary>
        public string Usuario { get; set; } = string.Empty;

        /// <summary>Identificador del rol asignado.</summary>
        public int IdRol { get; set; }

        /// <summary>Nombre descriptivo del rol.</summary>
        public string RolNombre { get; set; } = string.Empty;

        /// <summary>Estado del empleado (ACTIVO / INACTIVO).</summary>
        public string Estado { get; set; } = string.Empty;
    }
}
