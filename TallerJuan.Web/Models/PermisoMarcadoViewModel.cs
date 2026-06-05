namespace TallerJuan.Web.Models
{
    /// <summary>
    /// Representa un permiso del sistema junto con la marca de si está o no asignado
    /// al rol que se está editando. Alimenta cada checkbox de la pantalla de permisos.
    /// </summary>
    public class PermisoMarcadoViewModel
    {
        /// <summary>Identificador del permiso.</summary>
        public int IdPermiso { get; set; }

        /// <summary>Clave única del permiso (ej.: CLIENTES_VER).</summary>
        public string Clave { get; set; } = string.Empty;

        /// <summary>Descripción legible del permiso.</summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Módulo al que pertenece el permiso (para agrupar en la vista).</summary>
        public string Modulo { get; set; } = string.Empty;

        /// <summary>Indica si el rol actual ya tiene asignado este permiso.</summary>
        public bool Asignado { get; set; }
    }
}
