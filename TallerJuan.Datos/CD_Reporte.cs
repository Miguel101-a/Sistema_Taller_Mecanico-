using System.Data;
using Microsoft.Data.SqlClient;
using TallerJuan.Entidades;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Capa de Datos para los 5 reportes del ERS (RF-37 a RF-41). Acceso solo por
    /// PROCEDIMIENTOS ALMACENADOS (ADO.NET). PROHIBIDO Entity Framework. Solo lectura.
    /// </summary>
    public class CD_Reporte
    {
        /// <summary>RF-37: ingresos por período (DIA/MES/ANIO) sobre facturas EMITIDAS.</summary>
        public List<ReporteIngresoPeriodo> IngresosPorPeriodo(DateTime fechaInicio, DateTime fechaFin, string agrupacion)
        {
            List<ReporteIngresoPeriodo> filas = new List<ReporteIngresoPeriodo>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Reporte_IngresosPorPeriodo", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@FechaInicio", SqlDbType.DateTime) { Value = fechaInicio });
            comando.Parameters.Add(new SqlParameter("@FechaFin", SqlDbType.DateTime) { Value = fechaFin });
            comando.Parameters.Add(new SqlParameter("@Agrupacion", SqlDbType.VarChar, 10) { Value = agrupacion });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
            {
                filas.Add(new ReporteIngresoPeriodo
                {
                    Periodo = LectorBD.Texto(lector, "PERIODO"),
                    FechaOrden = LectorBD.Fecha(lector, "FECHA_ORDEN"),
                    CantidadFacturas = LectorBD.Entero(lector, "CANTIDAD_FACTURAS"),
                    Subtotal = LectorBD.Decimal(lector, "SUBTOTAL"),
                    Iva = LectorBD.Decimal(lector, "IVA"),
                    Total = LectorBD.Decimal(lector, "TOTAL")
                });
            }

            return filas;
        }

        /// <summary>RF-38: servicios más solicitados en facturas EMITIDAS del período.</summary>
        public List<ReporteServicioSolicitado> ServiciosMasSolicitados(DateTime fechaInicio, DateTime fechaFin)
        {
            List<ReporteServicioSolicitado> filas = new List<ReporteServicioSolicitado>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Reporte_ServiciosMasSolicitados", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@FechaInicio", SqlDbType.DateTime) { Value = fechaInicio });
            comando.Parameters.Add(new SqlParameter("@FechaFin", SqlDbType.DateTime) { Value = fechaFin });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
            {
                filas.Add(new ReporteServicioSolicitado
                {
                    Codigo = LectorBD.Texto(lector, "CODIGO"),
                    Nombre = LectorBD.Texto(lector, "NOMBRE"),
                    VecesSolicitado = LectorBD.Entero(lector, "VECES_SOLICITADO"),
                    MontoTotal = LectorBD.Decimal(lector, "MONTO_TOTAL")
                });
            }

            return filas;
        }

        /// <summary>RF-39: productividad por mecánico (órdenes atendidas y tiempos) en el período.</summary>
        public List<ReporteProductividadMecanico> ProductividadMecanico(DateTime fechaInicio, DateTime fechaFin)
        {
            List<ReporteProductividadMecanico> filas = new List<ReporteProductividadMecanico>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Reporte_ProductividadMecanico", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@FechaInicio", SqlDbType.DateTime) { Value = fechaInicio });
            comando.Parameters.Add(new SqlParameter("@FechaFin", SqlDbType.DateTime) { Value = fechaFin });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
            {
                filas.Add(new ReporteProductividadMecanico
                {
                    Cedula = LectorBD.Texto(lector, "CEDULA"),
                    Nombre = LectorBD.Texto(lector, "NOMBRE"),
                    OrdenesAtendidas = LectorBD.Entero(lector, "ORDENES_ATENDIDAS"),
                    OrdenesTerminadas = LectorBD.Entero(lector, "ORDENES_TERMINADAS"),
                    PromedioDiasEntrega = LectorBD.DecimalNulo(lector, "PROMEDIO_DIAS_ENTREGA")
                });
            }

            return filas;
        }

        /// <summary>RF-40: repuestos más utilizados (salidas de stock) en el período.</summary>
        public List<ReporteRepuestoUtilizado> RepuestosMasUtilizados(DateTime fechaInicio, DateTime fechaFin)
        {
            List<ReporteRepuestoUtilizado> filas = new List<ReporteRepuestoUtilizado>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Reporte_RepuestosMasUtilizados", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@FechaInicio", SqlDbType.DateTime) { Value = fechaInicio });
            comando.Parameters.Add(new SqlParameter("@FechaFin", SqlDbType.DateTime) { Value = fechaFin });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
            {
                filas.Add(new ReporteRepuestoUtilizado
                {
                    Codigo = LectorBD.Texto(lector, "CODIGO"),
                    Nombre = LectorBD.Texto(lector, "NOMBRE"),
                    StockActual = LectorBD.Entero(lector, "STOCK_ACTUAL"),
                    CantidadUtilizada = LectorBD.Entero(lector, "CANTIDAD_UTILIZADA"),
                    OrdenesDistintas = LectorBD.Entero(lector, "ORDENES_DISTINTAS")
                });
            }

            return filas;
        }

        /// <summary>RF-41: clientes frecuentes (órdenes y monto facturado) en el período.</summary>
        public List<ReporteClienteFrecuente> ClientesFrecuentes(DateTime fechaInicio, DateTime fechaFin)
        {
            List<ReporteClienteFrecuente> filas = new List<ReporteClienteFrecuente>();

            using SqlConnection conexion = ConexionBD.CrearConexion();
            using SqlCommand comando = new SqlCommand("sp_Reporte_ClientesFrecuentes", conexion)
            {
                CommandType = CommandType.StoredProcedure
            };

            comando.Parameters.Add(new SqlParameter("@FechaInicio", SqlDbType.DateTime) { Value = fechaInicio });
            comando.Parameters.Add(new SqlParameter("@FechaFin", SqlDbType.DateTime) { Value = fechaFin });

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
            {
                filas.Add(new ReporteClienteFrecuente
                {
                    Cedula = LectorBD.Texto(lector, "CEDULA"),
                    Nombre = LectorBD.Texto(lector, "NOMBRE"),
                    Telefono = LectorBD.Texto(lector, "TELEFONO"),
                    TotalOrdenes = LectorBD.Entero(lector, "TOTAL_ORDENES"),
                    MontoFacturado = LectorBD.Decimal(lector, "MONTO_FACTURADO")
                });
            }

            return filas;
        }
    }
}
