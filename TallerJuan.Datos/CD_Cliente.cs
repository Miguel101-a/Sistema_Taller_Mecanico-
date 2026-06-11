using System.Data;
using Microsoft.Data.SqlClient;
using TallerJuan.Entidades;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Capa de Datos para el CRUD de Clientes (Fase 4).
    /// Toda la comunicación con la base de datos se hace mediante PROCEDIMIENTOS ALMACENADOS
    /// usando ADO.NET (Microsoft.Data.SqlClient). PROHIBIDO Entity Framework.
    /// </summary>
    public class CD_Cliente
    {
        /// <summary>Ejecuta sp_Cliente_Listar y devuelve todos los clientes.</summary>
        public List<Cliente> Listar()
        {
            List<Cliente> clientes = new List<Cliente>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Cliente_Listar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
                clientes.Add(Mapear(lector));

            return clientes;
        }

        /// <summary>Ejecuta sp_Cliente_Obtener y devuelve un cliente por su cédula, o null si no existe.</summary>
        public Cliente? Obtener(string cedula)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Cliente_Obtener", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Cedula", SqlDbType.VarChar, 20) { Value = cedula });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            return lector.Read() ? Mapear(lector) : null;
        }

        /// <summary>
        /// Inserta un cliente mediante sp_Cliente_Insertar. Puede lanzar SqlException 2627/2601
        /// si la cédula viola la restricción de clave primaria; la capa de Negocio la traduce.
        /// </summary>
        public void Insertar(Cliente cliente)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Cliente_Insertar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            AgregarParametros(comando, cliente);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>Actualiza un cliente existente mediante sp_Cliente_Actualizar.</summary>
        public void Actualizar(Cliente cliente)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Cliente_Actualizar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            AgregarParametros(comando, cliente);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>Eliminación lógica de un cliente (ESTADO='INACTIVO') mediante sp_Cliente_Eliminar.</summary>
        public void Eliminar(string cedula)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Cliente_Eliminar", conexion)
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

        /// <summary>Agrega los parámetros comunes a insertar/actualizar (TELEFONO, DIRECCION, etc. nulos como DBNull).</summary>
        private static void AgregarParametros(SqlCommand comando, Cliente c)
        {
            comando.Parameters.Add(new SqlParameter("@Cedula", SqlDbType.VarChar, 20) { Value = c.Cedula });
            comando.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.VarChar, 100) { Value = c.Nombre });
            comando.Parameters.Add(new SqlParameter("@Telefono", SqlDbType.VarChar, 20) { Value = ValorONulo(c.Telefono) });
            comando.Parameters.Add(new SqlParameter("@Direccion", SqlDbType.VarChar, 200) { Value = ValorONulo(c.Direccion) });
            comando.Parameters.Add(new SqlParameter("@Correo", SqlDbType.VarChar, 100) { Value = ValorONulo(c.Correo) });
            comando.Parameters.Add(new SqlParameter("@Tipo", SqlDbType.VarChar, 20) { Value = ValorONulo(c.Tipo) });
        }

        /// <summary>Mapea la fila actual del lector a un objeto Cliente.</summary>
        private static Cliente Mapear(SqlDataReader lector)
        {
            return new Cliente
            {
                Cedula = LectorBD.Texto(lector, "CEDULA"),
                Nombre = LectorBD.Texto(lector, "NOMBRE"),
                Telefono = LectorBD.Texto(lector, "TELEFONO"),
                Direccion = LectorBD.Texto(lector, "DIRECCION"),
                Correo = LectorBD.Texto(lector, "CORREO"),
                Tipo = LectorBD.Texto(lector, "TIPO"),
                FechaRegistro = LectorBD.Fecha(lector, "FECHA_REGISTRO"),
                Estado = LectorBD.Texto(lector, "ESTADO")
            };
        }

        /// <summary>Convierte una cadena vacía en DBNull para columnas opcionales.</summary>
        private static object ValorONulo(string valor) =>
            string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
    }
}
