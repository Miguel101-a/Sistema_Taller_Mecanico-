using System.Data;
using Microsoft.Data.SqlClient;
using TallerJuan.Entidades;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Capa de Datos para la gestión de Roles y Permisos.
    /// Demuestra el manejo de la relación N:M ROL_PERMISO desde C#: asignar y quitar
    /// filas de la tabla intermedia, además del CRUD básico de roles.
    /// Toda la comunicación con la base de datos se hace mediante PROCEDIMIENTOS ALMACENADOS
    /// usando ADO.NET (Microsoft.Data.SqlClient). PROHIBIDO Entity Framework.
    /// </summary>
    public class CD_Seguridad
    {
        /// <summary>
        /// Ejecuta sp_Rol_ListarTodos y devuelve TODOS los roles (ACTIVO e INACTIVO),
        /// pensado para el panel de administración de Seguridad.
        /// </summary>
        public List<Rol> ListarTodosLosRoles()
        {
            List<Rol> roles = new List<Rol>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Rol_ListarTodos", conexion)
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
        /// Ejecuta sp_TodosLosPermisos y devuelve el catálogo completo de permisos del sistema.
        /// </summary>
        public List<Permiso> ListarTodosLosPermisos()
        {
            List<Permiso> permisos = new List<Permiso>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_TodosLosPermisos", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

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
        /// Ejecuta sp_PermisosPorRol y devuelve los permisos actualmente asignados a un rol
        /// (las filas de ROL_PERMISO para ese rol, resueltas a objetos Permiso).
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
        /// Asigna un permiso a un rol insertando la fila en ROL_PERMISO mediante sp_AsignarPermiso.
        /// El SP ya es idempotente (verifica con NOT EXISTS), por lo que aquí no se duplica esa protección.
        /// </summary>
        public void AsignarPermiso(int idRol, int idPermiso)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_AsignarPermiso", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@IdRol", SqlDbType.Int) { Value = idRol });
            comando.Parameters.Add(new SqlParameter("@IdPermiso", SqlDbType.Int) { Value = idPermiso });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>
        /// Quita un permiso de un rol eliminando la fila de ROL_PERMISO mediante sp_QuitarPermiso.
        /// Es un DELETE simple, naturalmente idempotente.
        /// </summary>
        public void QuitarPermiso(int idRol, int idPermiso)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_QuitarPermiso", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@IdRol", SqlDbType.Int) { Value = idRol });
            comando.Parameters.Add(new SqlParameter("@IdPermiso", SqlDbType.Int) { Value = idPermiso });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>
        /// Inserta un rol nuevo mediante sp_Rol_Insertar y devuelve el ID generado
        /// (el SP hace SELECT SCOPE_IDENTITY()). Puede lanzar SqlException 2627/2601
        /// si el nombre viola la restricción UNIQUE UQ_ROL_NOMBRE; la capa de Negocio la traduce.
        /// </summary>
        public int InsertarRol(string nombre)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Rol_Insertar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.VarChar, 50) { Value = nombre });

            conexion.Open();
            object? resultado = comando.ExecuteScalar();

            return resultado == null || resultado == DBNull.Value
                ? 0
                : Convert.ToInt32(resultado);
        }

        /// <summary>
        /// Renombra un rol mediante sp_Rol_Actualizar. Puede lanzar SqlException 2627/2601
        /// si el nuevo nombre choca con UQ_ROL_NOMBRE.
        /// </summary>
        public void ActualizarRol(int idRol, string nombre)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Rol_Actualizar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@IdRol", SqlDbType.Int) { Value = idRol });
            comando.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.VarChar, 50) { Value = nombre });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>
        /// Cambia el estado de un rol (ACTIVO / INACTIVO) mediante sp_Rol_CambiarEstado.
        /// Es un soft-delete; no se borra físicamente por integridad referencial.
        /// </summary>
        public void CambiarEstadoRol(int idRol, string estado)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Rol_CambiarEstado", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@IdRol", SqlDbType.Int) { Value = idRol });
            comando.Parameters.Add(new SqlParameter("@Estado", SqlDbType.VarChar, 10) { Value = estado });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        // ----------------------------------------------------------------------------------
        // Métodos auxiliares para leer columnas de forma segura (manejo de nulos de la BD).
        // Mismo patrón que CD_Acceso: se valida la existencia de la columna.
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
