namespace TallerJuan.Entidades
{
    /// <summary>
    /// Representa un rol del sistema (por ejemplo: Administrador, Mecánico, Recepcionista).
    /// </summary>
    public class Rol
    {
        /// <summary>Identificador del rol.</summary>
        public int IdRol { get; set; }

        /// <summary>Nombre descriptivo del rol.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Estado del rol (ACTIVO / INACTIVO).</summary>
        public string Estado { get; set; } = string.Empty;
    }
}
