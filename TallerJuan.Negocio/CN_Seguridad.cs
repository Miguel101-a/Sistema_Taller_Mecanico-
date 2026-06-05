using Microsoft.Data.SqlClient;
using TallerJuan.Datos;
using TallerJuan.Entidades;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Capa de Negocio para la gestión de Roles y Permisos (Fase 3).
    /// Envuelve a CD_Seguridad, aplica las reglas de negocio (protecciones del rol
    /// Administrador, validaciones, traducción de errores de BD) y registra la auditoría
    /// reutilizando CD_Acceso.RegistrarAuditoria. La capa Web SIEMPRE pasa por aquí.
    /// </summary>
    public class CN_Seguridad
    {
        // Identificador del rol Administrador (no puede desactivarse ni quedar sin ROLES_GESTIONAR).
        public const int IdRolAdministrador = 1;

        // Clave del permiso que da acceso a la gestión de roles; el admin no puede perderla.
        private const string ClaveRolesGestionar = "ROLES_GESTIONAR";

        // Módulo usado al registrar eventos en la auditoría.
        private const string ModuloSeguridad = "Seguridad";

        // Números de error de SQL Server para violación de restricción UNIQUE (UQ_ROL_NOMBRE).
        private const int ErrorUniqueIndex = 2601;
        private const int ErrorUniqueConstraint = 2627;

        private readonly CD_Seguridad _datos = new CD_Seguridad();
        private readonly CD_Acceso _acceso = new CD_Acceso();

        // ----------------------------------------------------------------------------------
        // Consultas
        // ----------------------------------------------------------------------------------

        /// <summary>Devuelve todos los roles (ACTIVO e INACTIVO) para el panel de administración.</summary>
        public List<Rol> ListarTodosLosRoles()
        {
            return _datos.ListarTodosLosRoles();
        }

        /// <summary>Devuelve el catálogo completo de permisos del sistema.</summary>
        public List<Permiso> ListarTodosLosPermisos()
        {
            return _datos.ListarTodosLosPermisos();
        }

        /// <summary>Devuelve los permisos actualmente asignados a un rol.</summary>
        public List<Permiso> ObtenerPermisosPorRol(int idRol)
        {
            return _datos.ObtenerPermisosPorRol(idRol);
        }

        // ----------------------------------------------------------------------------------
        // Gestión de permisos por rol (relación N:M ROL_PERMISO)
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// Asigna un permiso a un rol (inserta la fila en ROL_PERMISO) y registra la auditoría.
        /// </summary>
        public void AsignarPermiso(int idRol, int idPermiso, string usuarioAccion)
        {
            _datos.AsignarPermiso(idRol, idPermiso);
            _acceso.RegistrarAuditoria(usuarioAccion, ModuloSeguridad,
                $"Asignar permiso (IdPermiso={idPermiso}) al rol {idRol}");
        }

        /// <summary>
        /// Quita un permiso de un rol (elimina la fila de ROL_PERMISO) y registra la auditoría.
        /// Regla: no se permite quitar ROLES_GESTIONAR al rol Administrador (evita autobloqueo).
        /// </summary>
        public void QuitarPermiso(int idRol, int idPermiso, string usuarioAccion)
        {
            // Si se intenta quitar ROLES_GESTIONAR al Administrador, se bloquea la operación.
            if (idRol == IdRolAdministrador && EsPermisoRolesGestionar(idPermiso))
            {
                throw new InvalidOperationException(
                    "No se puede quitar el permiso 'Gestionar roles' al rol Administrador.");
            }

            _datos.QuitarPermiso(idRol, idPermiso);
            _acceso.RegistrarAuditoria(usuarioAccion, ModuloSeguridad,
                $"Quitar permiso (IdPermiso={idPermiso}) al rol {idRol}");
        }

        // ----------------------------------------------------------------------------------
        // CRUD de roles
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// Crea un rol nuevo. Valida que el nombre no esté vacío, traduce el error de nombre
        /// duplicado a un mensaje en español y registra la auditoría. Devuelve el ID generado.
        /// </summary>
        public int CrearRol(string nombre, string usuarioAccion)
        {
            string nombreLimpio = (nombre ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(nombreLimpio))
                throw new InvalidOperationException("El nombre del rol no puede estar vacío.");

            try
            {
                int idGenerado = _datos.InsertarRol(nombreLimpio);
                _acceso.RegistrarAuditoria(usuarioAccion, ModuloSeguridad,
                    $"Crear rol '{nombreLimpio}' (Id={idGenerado})");
                return idGenerado;
            }
            catch (SqlException ex) when (EsNombreDuplicado(ex))
            {
                throw new InvalidOperationException("Ya existe un rol con ese nombre.");
            }
        }

        /// <summary>
        /// Renombra un rol. Protege al Administrador (no puede quedar con nombre vacío),
        /// traduce el error de nombre duplicado y registra la auditoría.
        /// </summary>
        public void EditarRol(int idRol, string nombre, string usuarioAccion)
        {
            string nombreLimpio = (nombre ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(nombreLimpio))
                throw new InvalidOperationException("El nombre del rol no puede estar vacío.");

            try
            {
                _datos.ActualizarRol(idRol, nombreLimpio);
                _acceso.RegistrarAuditoria(usuarioAccion, ModuloSeguridad,
                    $"Editar rol {idRol} -> '{nombreLimpio}'");
            }
            catch (SqlException ex) when (EsNombreDuplicado(ex))
            {
                throw new InvalidOperationException("Ya existe un rol con ese nombre.");
            }
        }

        /// <summary>
        /// Cambia el estado de un rol (ACTIVO / INACTIVO). Reglas: el Administrador no puede
        /// desactivarse y el estado debe ser exactamente 'ACTIVO' o 'INACTIVO'. Audita la acción.
        /// </summary>
        public void CambiarEstadoRol(int idRol, string estado, string usuarioAccion)
        {
            string estadoNormalizado = (estado ?? string.Empty).Trim().ToUpperInvariant();

            if (estadoNormalizado != "ACTIVO" && estadoNormalizado != "INACTIVO")
                throw new InvalidOperationException("El estado del rol solo puede ser ACTIVO o INACTIVO.");

            // El rol Administrador nunca puede quedar inactivo.
            if (idRol == IdRolAdministrador && estadoNormalizado == "INACTIVO")
                throw new InvalidOperationException("El rol Administrador no se puede desactivar.");

            _datos.CambiarEstadoRol(idRol, estadoNormalizado);
            _acceso.RegistrarAuditoria(usuarioAccion, ModuloSeguridad,
                $"{(estadoNormalizado == "ACTIVO" ? "Activar" : "Desactivar")} rol {idRol}");
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Indica si el ID de permiso corresponde a la clave ROLES_GESTIONAR.</summary>
        private bool EsPermisoRolesGestionar(int idPermiso)
        {
            return _datos.ListarTodosLosPermisos()
                         .Any(p => p.IdPermiso == idPermiso && p.Clave == ClaveRolesGestionar);
        }

        /// <summary>Detecta si una SqlException corresponde a violación de la restricción UNIQUE del nombre.</summary>
        private static bool EsNombreDuplicado(SqlException ex)
        {
            return ex.Number == ErrorUniqueConstraint || ex.Number == ErrorUniqueIndex;
        }
    }
}
