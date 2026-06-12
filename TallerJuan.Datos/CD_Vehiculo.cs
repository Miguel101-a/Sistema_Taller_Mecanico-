using System.Data;
using Microsoft.Data.SqlClient;
using TallerJuan.Entidades;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Capa de Datos para el CRUD de Vehículos. Cada vehículo pertenece a un cliente.
    /// Acceso solo por PROCEDIMIENTOS ALMACENADOS (ADO.NET). PROHIBIDO Entity Framework.
    /// </summary>
    public class CD_Vehiculo
    {
        /// <summary>Ejecuta sp_Vehiculo_Listar (incluye CLIENTE_NOMBRE por JOIN).</summary>
        public List<Vehiculo> Listar()
        {
            List<Vehiculo> vehiculos = new List<Vehiculo>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Vehiculo_Listar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
                vehiculos.Add(Mapear(lector));

            return vehiculos;
        }

        /// <summary>Ejecuta sp_Vehiculo_Obtener y devuelve un vehículo por su placa, o null si no existe.</summary>
        public Vehiculo? Obtener(string placa)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Vehiculo_Obtener", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Placa", SqlDbType.VarChar, 10) { Value = placa });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            return lector.Read() ? Mapear(lector) : null;
        }

        /// <summary>
        /// Ejecuta sp_Vehiculo_PorCliente y devuelve los vehículos ACTIVOS de un cliente.
        /// Se usa para el formulario de creación de órdenes de trabajo.
        /// </summary>
        public List<Vehiculo> ListarPorCliente(string clienteCedula)
        {
            List<Vehiculo> vehiculos = new List<Vehiculo>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Vehiculo_PorCliente", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@ClienteCedula", SqlDbType.VarChar, 20) { Value = clienteCedula });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
            {
                vehiculos.Add(new Vehiculo
                {
                    Placa = LectorBD.Texto(lector, "PLACA"),
                    Marca = LectorBD.Texto(lector, "MARCA"),
                    Modelo = LectorBD.Texto(lector, "MODELO"),
                    Anio = LectorBD.EnteroNulo(lector, "ANIO"),
                    Color = LectorBD.Texto(lector, "COLOR"),
                    Kilometraje = LectorBD.EnteroNulo(lector, "KILOMETRAJE")
                });
            }

            return vehiculos;
        }

        /// <summary>
        /// Inserta un vehículo mediante sp_Vehiculo_Insertar. Puede lanzar SqlException 2627/2601
        /// si la placa viola la clave primaria; la capa de Negocio lo traduce.
        /// </summary>
        public void Insertar(Vehiculo vehiculo)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Vehiculo_Insertar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            AgregarParametros(comando, vehiculo);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>Actualiza un vehículo existente mediante sp_Vehiculo_Actualizar.</summary>
        public void Actualizar(Vehiculo vehiculo)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Vehiculo_Actualizar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            AgregarParametros(comando, vehiculo);

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>Eliminación lógica de un vehículo (ESTADO='INACTIVO') mediante sp_Vehiculo_Eliminar.</summary>
        public void Eliminar(string placa)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Vehiculo_Eliminar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Placa", SqlDbType.VarChar, 10) { Value = placa });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Agrega los parámetros comunes a insertar/actualizar (columnas opcionales como DBNull).</summary>
        private static void AgregarParametros(SqlCommand comando, Vehiculo v)
        {
            comando.Parameters.Add(new SqlParameter("@Placa", SqlDbType.VarChar, 10) { Value = v.Placa });
            comando.Parameters.Add(new SqlParameter("@Marca", SqlDbType.VarChar, 50) { Value = v.Marca });
            comando.Parameters.Add(new SqlParameter("@Modelo", SqlDbType.VarChar, 50) { Value = v.Modelo });
            comando.Parameters.Add(new SqlParameter("@Anio", SqlDbType.Int) { Value = (object?)v.Anio ?? DBNull.Value });
            comando.Parameters.Add(new SqlParameter("@Color", SqlDbType.VarChar, 30) { Value = ValorONulo(v.Color) });
            comando.Parameters.Add(new SqlParameter("@NumeroChasis", SqlDbType.VarChar, 50) { Value = ValorONulo(v.NumeroChasis) });
            comando.Parameters.Add(new SqlParameter("@NumeroMotor", SqlDbType.VarChar, 50) { Value = ValorONulo(v.NumeroMotor) });
            comando.Parameters.Add(new SqlParameter("@TipoCombustible", SqlDbType.VarChar, 20) { Value = ValorONulo(v.TipoCombustible) });
            comando.Parameters.Add(new SqlParameter("@Kilometraje", SqlDbType.Int) { Value = (object?)v.Kilometraje ?? DBNull.Value });
            comando.Parameters.Add(new SqlParameter("@ClienteCedula", SqlDbType.VarChar, 20) { Value = v.ClienteCedula });
        }

        /// <summary>Mapea la fila actual del lector a un objeto Vehiculo (con CLIENTE_NOMBRE).</summary>
        private static Vehiculo Mapear(SqlDataReader lector)
        {
            return new Vehiculo
            {
                Placa = LectorBD.Texto(lector, "PLACA"),
                Marca = LectorBD.Texto(lector, "MARCA"),
                Modelo = LectorBD.Texto(lector, "MODELO"),
                Anio = LectorBD.EnteroNulo(lector, "ANIO"),
                Color = LectorBD.Texto(lector, "COLOR"),
                NumeroChasis = LectorBD.Texto(lector, "NUMERO_CHASIS"),
                NumeroMotor = LectorBD.Texto(lector, "NUMERO_MOTOR"),
                TipoCombustible = LectorBD.Texto(lector, "TIPO_COMBUSTIBLE"),
                Kilometraje = LectorBD.EnteroNulo(lector, "KILOMETRAJE"),
                ClienteCedula = LectorBD.Texto(lector, "CLIENTE_CEDULA"),
                ClienteNombre = LectorBD.Texto(lector, "CLIENTE_NOMBRE"),
                Estado = LectorBD.Texto(lector, "ESTADO")
            };
        }

        /// <summary>Convierte una cadena vacía en DBNull para columnas opcionales.</summary>
        private static object ValorONulo(string valor) =>
            string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
    }
}
