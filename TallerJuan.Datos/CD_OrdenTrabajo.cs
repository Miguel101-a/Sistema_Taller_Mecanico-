using System.Data;
using Microsoft.Data.SqlClient;
using TallerJuan.Entidades;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Capa de Datos para las Órdenes de Trabajo. Acceso solo por PROCEDIMIENTOS
    /// ALMACENADOS (ADO.NET). PROHIBIDO Entity Framework.
    /// </summary>
    public class CD_OrdenTrabajo
    {
        /// <summary>Ejecuta sp_OrdenTrabajo_Listar (incluye datos de vehículo, cliente y mecánico).</summary>
        public List<OrdenTrabajo> Listar()
        {
            List<OrdenTrabajo> ordenes = new List<OrdenTrabajo>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_OrdenTrabajo_Listar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
                ordenes.Add(Mapear(lector));

            return ordenes;
        }

        /// <summary>Ejecuta sp_OrdenTrabajo_Obtener y devuelve una orden por su número, o null si no existe.</summary>
        public OrdenTrabajo? Obtener(int numeroOrden)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_OrdenTrabajo_Obtener", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroOrden", SqlDbType.Int) { Value = numeroOrden });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            return lector.Read() ? Mapear(lector) : null;
        }

        /// <summary>
        /// Inserta una orden mediante sp_OrdenTrabajo_Insertar y devuelve el NUMERO_ORDEN generado
        /// (el SP hace SELECT SCOPE_IDENTITY()).
        /// </summary>
        public int Insertar(OrdenTrabajo orden)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_OrdenTrabajo_Insertar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@KilometrajeIngreso", SqlDbType.Int) { Value = (object?)orden.KilometrajeIngreso ?? DBNull.Value });
            comando.Parameters.Add(new SqlParameter("@MotivoVisita", SqlDbType.VarChar, 200) { Value = ValorONulo(orden.MotivoVisita) });
            comando.Parameters.Add(new SqlParameter("@DescripcionProblema", SqlDbType.VarChar, 500) { Value = ValorONulo(orden.DescripcionProblema) });
            comando.Parameters.Add(new SqlParameter("@VehiculoPlaca", SqlDbType.VarChar, 10) { Value = orden.VehiculoPlaca });
            comando.Parameters.Add(new SqlParameter("@EmpleadoCedula", SqlDbType.VarChar, 20) { Value = orden.EmpleadoCedula });

            conexion.Open();
            object? resultado = comando.ExecuteScalar();

            return resultado == null || resultado == DBNull.Value
                ? 0
                : Convert.ToInt32(resultado);
        }

        /// <summary>Actualiza los datos editables de una orden mediante sp_OrdenTrabajo_Actualizar.</summary>
        public void Actualizar(OrdenTrabajo orden)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_OrdenTrabajo_Actualizar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroOrden", SqlDbType.Int) { Value = orden.NumeroOrden });
            comando.Parameters.Add(new SqlParameter("@KilometrajeIngreso", SqlDbType.Int) { Value = (object?)orden.KilometrajeIngreso ?? DBNull.Value });
            comando.Parameters.Add(new SqlParameter("@MotivoVisita", SqlDbType.VarChar, 200) { Value = ValorONulo(orden.MotivoVisita) });
            comando.Parameters.Add(new SqlParameter("@DescripcionProblema", SqlDbType.VarChar, 500) { Value = ValorONulo(orden.DescripcionProblema) });
            comando.Parameters.Add(new SqlParameter("@EmpleadoCedula", SqlDbType.VarChar, 20) { Value = orden.EmpleadoCedula });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        /// <summary>Cambia el estado de una orden mediante sp_OrdenTrabajo_CambiarEstado.</summary>
        public void CambiarEstado(int numeroOrden, string estado)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_OrdenTrabajo_CambiarEstado", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroOrden", SqlDbType.Int) { Value = numeroOrden });
            comando.Parameters.Add(new SqlParameter("@Estado", SqlDbType.VarChar, 30) { Value = estado });

            conexion.Open();
            comando.ExecuteNonQuery();
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Mapea la fila actual del lector a un objeto OrdenTrabajo.</summary>
        private static OrdenTrabajo Mapear(SqlDataReader lector)
        {
            return new OrdenTrabajo
            {
                NumeroOrden = LectorBD.Entero(lector, "NUMERO_ORDEN"),
                FechaIngreso = LectorBD.Fecha(lector, "FECHA_INGRESO"),
                KilometrajeIngreso = LectorBD.EnteroNulo(lector, "KILOMETRAJE_INGRESO"),
                MotivoVisita = LectorBD.Texto(lector, "MOTIVO_VISITA"),
                DescripcionProblema = LectorBD.Texto(lector, "DESCRIPCION_PROBLEMA"),
                Estado = LectorBD.Texto(lector, "ESTADO"),
                FechaEntrega = LectorBD.FechaNula(lector, "FECHA_ENTREGA"),
                VehiculoPlaca = LectorBD.Texto(lector, "VEHICULO_PLACA"),
                VehiculoDescripcion = LectorBD.Texto(lector, "VEHICULO_DESCRIPCION"),
                ClienteCedula = LectorBD.Texto(lector, "CLIENTE_CEDULA"),
                ClienteNombre = LectorBD.Texto(lector, "CLIENTE_NOMBRE"),
                EmpleadoCedula = LectorBD.Texto(lector, "EMPLEADO_CEDULA"),
                EmpleadoNombre = LectorBD.Texto(lector, "EMPLEADO_NOMBRE")
            };
        }

        /// <summary>Convierte una cadena vacía en DBNull para columnas opcionales.</summary>
        private static object ValorONulo(string valor) =>
            string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
    }
}
