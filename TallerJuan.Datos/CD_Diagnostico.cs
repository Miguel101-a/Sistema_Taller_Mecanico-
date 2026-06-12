using System.Data;
using Microsoft.Data.SqlClient;
using TallerJuan.Entidades;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Capa de Datos para los Diagnósticos de una orden de trabajo.
    /// Acceso solo por PROCEDIMIENTOS ALMACENADOS (ADO.NET). PROHIBIDO Entity Framework.
    /// </summary>
    public class CD_Diagnostico
    {
        /// <summary>Ejecuta sp_Diagnostico_PorOrden y devuelve los diagnósticos de una orden (más recientes primero).</summary>
        public List<Diagnostico> ListarPorOrden(int numeroOrden)
        {
            List<Diagnostico> diagnosticos = new List<Diagnostico>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Diagnostico_PorOrden", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@NumeroOrden", SqlDbType.Int) { Value = numeroOrden });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
            {
                diagnosticos.Add(new Diagnostico
                {
                    IdDiagnostico = LectorBD.Entero(lector, "ID_DIAGNOSTICO"),
                    Descripcion = LectorBD.Texto(lector, "DESCRIPCION"),
                    TiempoEstimado = LectorBD.DecimalNulo(lector, "TIEMPO_ESTIMADO"),
                    CostoEstimado = LectorBD.DecimalNulo(lector, "COSTO_ESTIMADO"),
                    Fecha = LectorBD.Fecha(lector, "FECHA"),
                    OrdenTrabajoNumeroOrden = LectorBD.Entero(lector, "ORDEN_TRABAJO_NUMERO_ORDEN")
                });
            }

            return diagnosticos;
        }

        /// <summary>
        /// Inserta un diagnóstico mediante sp_Diagnostico_Insertar y devuelve el ID_DIAGNOSTICO generado.
        /// </summary>
        public int Insertar(Diagnostico diagnostico)
        {
            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Diagnostico_Insertar", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.VarChar, 1000) { Value = diagnostico.Descripcion });
            comando.Parameters.Add(new SqlParameter("@TiempoEstimado", SqlDbType.Decimal) { Value = (object?)diagnostico.TiempoEstimado ?? DBNull.Value });
            comando.Parameters.Add(new SqlParameter("@CostoEstimado", SqlDbType.Decimal) { Value = (object?)diagnostico.CostoEstimado ?? DBNull.Value });
            comando.Parameters.Add(new SqlParameter("@NumeroOrden", SqlDbType.Int) { Value = diagnostico.OrdenTrabajoNumeroOrden });

            conexion.Open();
            object? resultado = comando.ExecuteScalar();

            return resultado == null || resultado == DBNull.Value
                ? 0
                : Convert.ToInt32(resultado);
        }
    }
}
