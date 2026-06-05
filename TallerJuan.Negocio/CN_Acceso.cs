using TallerJuan.Datos;
using TallerJuan.Entidades;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Capa de Negocio para el acceso al sistema. Orquesta la autenticación, el cálculo del hash,
    /// la obtención de permisos y el registro de auditoría. La capa Web SIEMPRE pasa por aquí,
    /// nunca llama a la capa de Datos (CD_) directamente.
    /// </summary>
    public class CN_Acceso
    {
        // Módulo usado al registrar eventos de auditoría de acceso.
        private const string ModuloSeguridad = "Seguridad";

        private readonly CD_Acceso _datos = new CD_Acceso();

        /// <summary>
        /// Inicia sesión: calcula el hash SHA-256 de la clave en texto plano, valida las credenciales
        /// contra la base de datos y, si son correctas, registra la auditoría de inicio de sesión.
        /// Devuelve el Empleado autenticado o null si las credenciales no son válidas.
        /// </summary>
        public Empleado? IniciarSesion(string usuario, string clavePlana)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clavePlana))
                return null;

            // Se calcula el hash de la clave para compararlo con el almacenado en la BD.
            string hashClave = Seguridad.HashSHA256(clavePlana);

            Empleado? empleado = _datos.Login(usuario.Trim(), hashClave);

            if (empleado != null)
            {
                // Login exitoso: se deja constancia en la bitácora.
                _datos.RegistrarAuditoria(empleado.Usuario, ModuloSeguridad, "Inicio de sesión");
            }

            return empleado;
        }

        /// <summary>
        /// Devuelve la lista completa de permisos (objetos Permiso) asociados a un rol.
        /// </summary>
        public List<Permiso> ObtenerPermisosPorRol(int idRol)
        {
            return _datos.ObtenerPermisosPorRol(idRol);
        }

        /// <summary>
        /// Devuelve únicamente las CLAVES de permiso del rol. Es lo que necesita la capa Web
        /// para mostrar u ocultar opciones del menú.
        /// </summary>
        public List<string> ObtenerClavesPermiso(int idRol)
        {
            return _datos.ObtenerPermisosPorRol(idRol)
                         .Select(p => p.Clave)
                         .ToList();
        }

        /// <summary>
        /// Registra en la auditoría el cierre de sesión de un usuario.
        /// </summary>
        public void RegistrarCierreSesion(string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
                return;

            _datos.RegistrarAuditoria(usuario, ModuloSeguridad, "Cierre de sesión");
        }
    }
}
