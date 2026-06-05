namespace TallerJuan.Web.Models
{
    /// <summary>
    /// Modelo de la pantalla "Gestionar permisos" de un rol: nombre del rol y la lista
    /// completa de permisos del sistema, cada uno marcado según esté o no asignado.
    /// La vista los agrupa por MODULO para presentarlos en tarjetas.
    /// </summary>
    public class PermisosRolViewModel
    {
        /// <summary>Identificador del rol que se está editando.</summary>
        public int IdRol { get; set; }

        /// <summary>Nombre del rol que se está editando (se muestra en el encabezado).</summary>
        public string NombreRol { get; set; } = string.Empty;

        /// <summary>Catálogo completo de permisos con la marca de asignado para este rol.</summary>
        public List<PermisoMarcadoViewModel> Permisos { get; set; } = new List<PermisoMarcadoViewModel>();
    }
}
