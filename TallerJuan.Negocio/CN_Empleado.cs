using Microsoft.Data.SqlClient;
using TallerJuan.Datos;
using TallerJuan.Entidades;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Capa de Negocio para Empleados (Fase 4). El empleado es también el usuario del sistema.
    /// Al crear, hashea la contraseña con Seguridad.HashSHA256 antes de llamar a Datos.
    /// Al editar NO se cambia usuario ni contraseña. Un empleado no puede desactivarse a sí mismo.
    /// Valida, traduce errores de BD y registra la auditoría.
    /// </summary>
    public class CN_Empleado
    {
        private const string Modulo = "Empleados";
        private const int LongitudMinimaClave = 4;
        private const int ErrorUniqueIndex = 2601;
        private const int ErrorUniqueConstraint = 2627;

        private readonly CD_Empleado _datos = new CD_Empleado();
        private readonly CD_Acceso _acceso = new CD_Acceso();

        /// <summary>Devuelve todos los empleados (con el nombre del rol).</summary>
        public List<Empleado> Listar() => _datos.Listar();

        /// <summary>Devuelve un empleado por su cédula, o null si no existe.</summary>
        public Empleado? Obtener(string cedula) => _datos.Obtener(cedula);

        /// <summary>Devuelve los roles ACTIVOS para el dropdown de creación/edición (sp_ListarRoles).</summary>
        public List<Rol> ListarRolesActivos() => _acceso.ListarRoles();

        /// <summary>
        /// Crea un empleado nuevo. Valida los datos y la contraseña (mínimo 4 caracteres),
        /// calcula el hash SHA-256, traduce cédula/usuario duplicados y registra la auditoría.
        /// </summary>
        public void Crear(Empleado empleado, string clavePlana, string usuarioAccion)
        {
            Normalizar(empleado);
            Validar(empleado);

            if (string.IsNullOrWhiteSpace(empleado.Usuario))
                throw new InvalidOperationException("El nombre de usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(clavePlana) || clavePlana.Length < LongitudMinimaClave)
                throw new InvalidOperationException($"La contraseña debe tener al menos {LongitudMinimaClave} caracteres.");

            // El hash se calcula en la capa de Negocio antes de llegar a Datos.
            string hashClave = Seguridad.HashSHA256(clavePlana);

            try
            {
                _datos.Insertar(empleado, hashClave);
                _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Crear empleado {empleado.Cedula} (usuario {empleado.Usuario})");
            }
            catch (SqlException ex) when (EsDuplicado(ex))
            {
                throw new InvalidOperationException("Ya existe un empleado con esa cédula o ese usuario.");
            }
        }

        /// <summary>
        /// Actualiza los datos de un empleado (sin tocar usuario ni contraseña).
        /// Valida y registra la auditoría.
        /// </summary>
        public void Editar(Empleado empleado, string usuarioAccion)
        {
            Normalizar(empleado);
            Validar(empleado);

            _datos.Actualizar(empleado);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Editar empleado {empleado.Cedula}");
        }

        /// <summary>
        /// Desactiva un empleado (eliminación lógica). Regla: un empleado no puede desactivarse
        /// a sí mismo (se compara con la cédula del usuario en sesión). Registra la auditoría.
        /// </summary>
        public void Eliminar(string cedula, string cedulaUsuarioActual, string usuarioAccion)
        {
            if (!string.IsNullOrWhiteSpace(cedulaUsuarioActual) &&
                string.Equals(cedula?.Trim(), cedulaUsuarioActual.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("No puede desactivar su propio usuario.");
            }

            _datos.Eliminar(cedula!);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Desactivar empleado {cedula}");
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Recorta los espacios de los campos de texto.</summary>
        private static void Normalizar(Empleado e)
        {
            e.Cedula = (e.Cedula ?? string.Empty).Trim();
            e.Nombre = (e.Nombre ?? string.Empty).Trim();
            e.Telefono = (e.Telefono ?? string.Empty).Trim();
            e.Direccion = (e.Direccion ?? string.Empty).Trim();
            e.Cargo = (e.Cargo ?? string.Empty).Trim();
            e.Especialidad = (e.Especialidad ?? string.Empty).Trim();
            e.Usuario = (e.Usuario ?? string.Empty).Trim();
        }

        /// <summary>Valida los campos obligatorios y los rangos de un empleado.</summary>
        private static void Validar(Empleado e)
        {
            if (string.IsNullOrWhiteSpace(e.Cedula))
                throw new InvalidOperationException("La cédula del empleado es obligatoria.");
            if (string.IsNullOrWhiteSpace(e.Nombre))
                throw new InvalidOperationException("El nombre del empleado es obligatorio.");
            if (string.IsNullOrWhiteSpace(e.Cargo))
                throw new InvalidOperationException("El cargo del empleado es obligatorio.");
            if (e.IdRol <= 0)
                throw new InvalidOperationException("Debe seleccionar un rol para el empleado.");
            if (e.Salario.HasValue && e.Salario < 0)
                throw new InvalidOperationException("El salario no puede ser negativo.");
        }

        /// <summary>Detecta violación de clave primaria/única (cédula o usuario duplicados).</summary>
        private static bool EsDuplicado(SqlException ex) =>
            ex.Number == ErrorUniqueConstraint || ex.Number == ErrorUniqueIndex;
    }
}
