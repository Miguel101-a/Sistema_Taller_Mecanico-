using System.Data;
using Microsoft.Data.SqlClient;
using TallerJuan.Entidades;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Capa de Datos para el CRUD de Empleados. El empleado es también el usuario del sistema.
    /// La contraseña llega ya hasheada desde la capa de Negocio. Acceso solo por PROCEDIMIENTOS
    /// ALMACENADOS (ADO.NET). PROHIBIDO Entity Framework.
    /// </summary>
    public class CD_Empleado
    {
        /// <summary>Ejecuta sp_Empleado_Listar (incluye ROL_NOMBRE por JOIN).</summary>
        public List<Empleado> Listar()
        {
            List<Empleado> empleados = new List<Empleado>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Empleado_Listar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
                empleados.Add(Mapear(lector));

            return empleados;
        }

        /// <summary>Ejecuta sp_Empleado_Obtener y devuelve un empleado por su cédula, o null si no existe.</summary>
        public Empleado? Obtener(string cedula)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Empleado_Obtener", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Cedula", SqlDbType.VarChar, 20) { Value = cedula });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            return lector.Read() ? Mapear(lector) : null;
        }

        /// <summary>
        /// Inserta un empleado mediante sp_Empleado_Insertar. Recibe el hash de la contraseña
        /// ya calculado por la capa de Negocio (@HashClave). Puede lanzar SqlException 2627/2601
        /// si la cédula o el usuario (UQ_EMPLEADO_USUARIO) ya existen.
        /// </summary>
        public void Insertar(Empleado empleado, string hashClave)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Empleado_Insertar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Cedula", SqlDbType.VarChar, 20) { Value = empleado.Cedula });
            comando.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.VarChar, 100) { Value = empleado.Nombre });
            comando.Parameters.Add(new SqlParameter("@Telefono", SqlDbType.VarChar, 20) { Value = ValorONulo(empleado.Telefono) });
            comando.Parameters.Add(new SqlParameter("@Direccion", SqlDbType.VarChar, 200) { Value = ValorONulo(empleado.Direccion) });
            comando.Parameters.Add(new SqlParameter("@Cargo", SqlDbType.VarChar, 30) { Value = empleado.Cargo });
            comando.Parameters.Add(new SqlParameter("@Salario", SqlDbType.Decimal) { Value = (object?)empleado.Salario ?? DBNull.Value });
            comando.Parameters.Add(new SqlParameter("@Especialidad", SqlDbType.VarChar, 100) { Value = ValorONulo(empleado.Especialidad) });
            comando.Parameters.Add(new SqlParameter("@Usuario", SqlDbType.VarChar, 50) { Value = empleado.Usuario });
            comando.Parameters.Add(new SqlParameter("@HashClave", SqlDbType.VarChar, 64) { Value = hashClave });
            comando.Parameters.Add(new SqlParameter("@IdRol", SqlDbType.Int) { Value = empleado.IdRol });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>
        /// Actualiza los datos de un empleado mediante sp_Empleado_Actualizar.
        /// NO actualiza usuario ni contraseña (fuera del alcance de esta fase).
        /// </summary>
        public void Actualizar(Empleado empleado)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Empleado_Actualizar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Cedula", SqlDbType.VarChar, 20) { Value = empleado.Cedula });
            comando.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.VarChar, 100) { Value = empleado.Nombre });
            comando.Parameters.Add(new SqlParameter("@Telefono", SqlDbType.VarChar, 20) { Value = ValorONulo(empleado.Telefono) });
            comando.Parameters.Add(new SqlParameter("@Direccion", SqlDbType.VarChar, 200) { Value = ValorONulo(empleado.Direccion) });
            comando.Parameters.Add(new SqlParameter("@Cargo", SqlDbType.VarChar, 30) { Value = empleado.Cargo });
            comando.Parameters.Add(new SqlParameter("@Salario", SqlDbType.Decimal) { Value = (object?)empleado.Salario ?? DBNull.Value });
            comando.Parameters.Add(new SqlParameter("@Especialidad", SqlDbType.VarChar, 100) { Value = ValorONulo(empleado.Especialidad) });
            comando.Parameters.Add(new SqlParameter("@IdRol", SqlDbType.Int) { Value = empleado.IdRol });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>Eliminación lógica de un empleado (ESTADO='INACTIVO') mediante sp_Empleado_Eliminar.</summary>
        public void Eliminar(string cedula)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Empleado_Eliminar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Cedula", SqlDbType.VarChar, 20) { Value = cedula });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Mapea la fila actual del lector a un objeto Empleado (con ROL_NOMBRE).</summary>
        private static Empleado Mapear(SqlDataReader lector)
        {
            return new Empleado
            {
                Cedula = LectorBD.Texto(lector, "CEDULA"),
                Nombre = LectorBD.Texto(lector, "NOMBRE"),
                Telefono = LectorBD.Texto(lector, "TELEFONO"),
                Direccion = LectorBD.Texto(lector, "DIRECCION"),
                Cargo = LectorBD.Texto(lector, "CARGO"),
                FechaIngreso = LectorBD.Fecha(lector, "FECHA_INGRESO"),
                Salario = LectorBD.DecimalNulo(lector, "SALARIO"),
                Especialidad = LectorBD.Texto(lector, "ESPECIALIDAD"),
                Usuario = LectorBD.Texto(lector, "USUARIO"),
                IdRol = LectorBD.Entero(lector, "ID_ROL"),
                RolNombre = LectorBD.Texto(lector, "ROL_NOMBRE"),
                Estado = LectorBD.Texto(lector, "ESTADO")
            };
        }

        /// <summary>Convierte una cadena vacía en DBNull para columnas opcionales.</summary>
        private static object ValorONulo(string valor) =>
            string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
    }
}
