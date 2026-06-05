using System.Data;
using Microsoft.Data.SqlClient;
using TallerJuan.Entidades;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Capa de Datos para el acceso al sistema: autenticación, permisos, roles y auditoría.
    /// Toda la comunicación con la base de datos se hace mediante PROCEDIMIENTOS ALMACENADOS
    /// usando ADO.NET (Microsoft.Data.SqlClient). PROHIBIDO Entity Framework.
    /// </summary>
    public class CD_Acceso
    {
        /// <summary>
        /// Valida las credenciales ejecutando sp_Login. Recibe el usuario y el hash de la clave.
        /// Devuelve el Empleado autenticado, o null si las credenciales no coinciden
        /// o el empleado no está activo.
        /// </summary>
        public Empleado? Login(string usuario, string hashClave)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Login", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Usuario", SqlDbType.VarChar, 50) { Value = usuario });
            comando.Parameters.Add(new SqlParameter("@HashClave", SqlDbType.VarChar, 64) { Value = hashClave });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            if (!lector.Read())
                return null; // No hubo coincidencia: credenciales inválidas o empleado inactivo.

            return new Empleado
            {
                Cedula = LeerTexto(lector, "CEDULA"),
                Nombre = LeerTexto(lector, "NOMBRE"),
                Usuario = LeerTexto(lector, "USUARIO"),
                Cargo = LeerTexto(lector, "CARGO"),
                IdRol = LeerEntero(lector, "ID_ROL"),
                RolNombre = LeerTexto(lector, "ROL_NOMBRE")
            };
        }

        /// <summary>
        /// Ejecuta sp_PermisosPorRol y devuelve la lista de permisos asociados a un rol.
        /// </summary>
        public List<Permiso> ObtenerPermisosPorRol(int idRol)
        {
            List<Permiso> permisos = new List<Permiso>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_PermisosPorRol", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@IdRol", SqlDbType.Int) { Value = idRol });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
            {
                permisos.Add(new Permiso
                {
                    IdPermiso = LeerEntero(lector, "ID_PERMISO"),
                    Clave = LeerTexto(lector, "CLAVE"),
                    Descripcion = LeerTexto(lector, "DESCRIPCION"),
                    Modulo = LeerTexto(lector, "MODULO")
                });
            }

            return permisos;
        }

        /// <summary>
        /// Ejecuta sp_ListarRoles y devuelve la lista de roles activos del sistema.
        /// </summary>
        public List<Rol> ListarRoles()
        {
            List<Rol> roles = new List<Rol>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_ListarRoles", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
            {
                roles.Add(new Rol
                {
                    IdRol = LeerEntero(lector, "ID_ROL"),
                    Nombre = LeerTexto(lector, "NOMBRE"),
                    Estado = LeerTexto(lector, "ESTADO")
                });
            }

            return roles;
        }

        /// <summary>
        /// Ejecuta sp_Auditoria_Registrar para dejar constancia de una operación en la bitácora.
        /// </summary>
        public void RegistrarAuditoria(string usuario, string modulo, string operacion)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Auditoria_Registrar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Usuario", SqlDbType.VarChar, 50) { Value = usuario });
            comando.Parameters.Add(new SqlParameter("@Modulo", SqlDbType.VarChar, 100) { Value = modulo });
            comando.Parameters.Add(new SqlParameter("@Operacion", SqlDbType.VarChar, 200) { Value = operacion });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        // ----------------------------------------------------------------------------------
        // Métodos auxiliares para leer columnas de forma segura (manejo de nulos de la BD).
        // Se valida la existencia de la columna para tolerar variaciones en los resultados.
        // ----------------------------------------------------------------------------------

        private static string LeerTexto(SqlDataReader lector, string columna)
        {
            int indice = ObtenerIndice(lector, columna);
            if (indice < 0 || lector.IsDBNull(indice))
                return string.Empty;
            return lector.GetValue(indice)?.ToString() ?? string.Empty;
        }

        private static int LeerEntero(SqlDataReader lector, string columna)
        {
            int indice = ObtenerIndice(lector, columna);
            if (indice < 0 || lector.IsDBNull(indice))
                return 0;
            return Convert.ToInt32(lector.GetValue(indice));
        }

        private static int ObtenerIndice(SqlDataReader lector, string columna)
        {
            try
            {
                return lector.GetOrdinal(columna);
            }
            catch (IndexOutOfRangeException)
            {
                return -1; // La columna no existe en el resultado.
            }
        }
    }
}
