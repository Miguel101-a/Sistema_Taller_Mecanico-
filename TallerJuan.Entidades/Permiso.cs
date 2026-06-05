namespace TallerJuan.Entidades
{
    /// <summary>
    /// Representa un permiso del sistema. Se usa para mostrar u ocultar opciones del menú
    /// y para autorizar operaciones según el rol del usuario.
    /// </summary>
    public class Permiso
    {
        /// <summary>Identificador del permiso.</summary>
        public int IdPermiso { get; set; }

        /// <summary>Clave única del permiso (ej.: CLIENTES_VER). Se usa para condicionar el menú.</summary>
        public string Clave { get; set; } = string.Empty;

        /// <summary>Descripción legible del permiso.</summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Módulo al que pertenece el permiso.</summary>
        public string Modulo { get; set; } = string.Empty;
    }
}
