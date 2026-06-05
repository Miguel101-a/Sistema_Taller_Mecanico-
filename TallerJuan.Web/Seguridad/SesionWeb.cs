using System.Text.Json;
using TallerJuan.Entidades;

namespace TallerJuan.Web.Seguridad
{
    /// <summary>
    /// Utilitario para guardar y leer los datos del usuario autenticado en la sesión web.
    /// Centraliza las claves de sesión y la (de)serialización de la lista de permisos,
    /// de modo que controladores y vistas no manipulen la sesión "a mano".
    /// </summary>
    public static class SesionWeb
    {
        // Claves con las que se almacenan los valores en la sesión.
        public const string ClaveCedula = "Cedula";
        public const string ClaveNombre = "Nombre";
        public const string ClaveUsuario = "Usuario";
        public const string ClaveIdRol = "IdRol";
        public const string ClaveRolNombre = "RolNombre";
        public const string ClavePermisos = "Permisos";

        /// <summary>
        /// Guarda en la sesión los datos del empleado autenticado y sus claves de permiso.
        /// </summary>
        public static void IniciarSesion(ISession sesion, Empleado empleado, List<string> clavesPermiso)
        {
            sesion.SetString(ClaveCedula, empleado.Cedula);
            sesion.SetString(ClaveNombre, empleado.Nombre);
            sesion.SetString(ClaveUsuario, empleado.Usuario);
            sesion.SetInt32(ClaveIdRol, empleado.IdRol);
            sesion.SetString(ClaveRolNombre, empleado.RolNombre);
            sesion.SetString(ClavePermisos, JsonSerializer.Serialize(clavesPermiso));
        }

        /// <summary>Indica si hay un usuario con sesión iniciada.</summary>
        public static bool HaySesion(ISession sesion)
        {
            return !string.IsNullOrEmpty(sesion.GetString(ClaveUsuario));
        }

        /// <summary>Devuelve el nombre del usuario en sesión (o cadena vacía).</summary>
        public static string Usuario(ISession sesion) => sesion.GetString(ClaveUsuario) ?? string.Empty;

        /// <summary>Devuelve el nombre completo del usuario en sesión (o cadena vacía).</summary>
        public static string Nombre(ISession sesion) => sesion.GetString(ClaveNombre) ?? string.Empty;

        /// <summary>Devuelve el nombre del rol del usuario en sesión (o cadena vacía).</summary>
        public static string RolNombre(ISession sesion) => sesion.GetString(ClaveRolNombre) ?? string.Empty;

        /// <summary>Devuelve la lista de claves de permiso del usuario en sesión.</summary>
        public static List<string> Permisos(ISession sesion)
        {
            string? json = sesion.GetString(ClavePermisos);
            if (string.IsNullOrEmpty(json))
                return new List<string>();

            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }

        /// <summary>
        /// Indica si el usuario en sesión posee la clave de permiso indicada.
        /// Se usa en el menú para mostrar u ocultar cada opción.
        /// </summary>
        public static bool TienePermiso(ISession sesion, string clave)
        {
            return Permisos(sesion).Contains(clave);
        }
    }
}
