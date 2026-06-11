using TallerJuan.Entidades;

namespace TallerJuan.Web.Models
{
    /// <summary>
    /// ViewModel para el alta de empleados. Incluye el campo de contraseña en texto plano
    /// (que la capa de Negocio hashea) y la lista de roles activos para el dropdown.
    /// La contraseña NO es una propiedad de la entidad Empleado por seguridad.
    /// </summary>
    public class EmpleadoCrearViewModel
    {
        /// <summary>Datos del empleado a crear.</summary>
        public Empleado Empleado { get; set; } = new Empleado();

        /// <summary>Contraseña en texto plano (mínimo 4 caracteres). Se hashea en la capa de Negocio.</summary>
        public string Contrasena { get; set; } = string.Empty;

        /// <summary>Roles ACTIVOS disponibles para asignar al empleado.</summary>
        public List<Rol> Roles { get; set; } = new List<Rol>();
    }
}
