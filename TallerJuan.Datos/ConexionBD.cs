using Microsoft.Data.SqlClient;

namespace TallerJuan.Datos
{
    /// <summary>
    /// Clase responsable de proveer la cadena de conexión y crear objetos SqlConnection.
    /// La cadena de conexión se inyecta UNA sola vez al iniciar la aplicación desde la capa Web,
    /// de modo que la capa Datos no dependa de ASP.NET Core ni de IConfiguration.
    /// </summary>
    public static class ConexionBD
    {
        // Campo estático donde se guarda la cadena de conexión durante toda la vida de la app.
        private static string? _cadenaConexion;

        /// <summary>
        /// Configura la cadena de conexión. Debe llamarse UNA sola vez al iniciar la app (desde Program.cs).
        /// </summary>
        public static void Configurar(string cadenaConexion)
        {
            if (string.IsNullOrWhiteSpace(cadenaConexion))
                throw new ArgumentException("La cadena de conexión no puede estar vacía.", nameof(cadenaConexion));

            _cadenaConexion = cadenaConexion;
        }

        /// <summary>
        /// Devuelve la cadena de conexión configurada. Lanza excepción si aún no se configuró.
        /// </summary>
        public static string CadenaConexion
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_cadenaConexion))
                    throw new InvalidOperationException(
                        "La cadena de conexión no ha sido configurada. Llame a ConexionBD.Configurar() al iniciar la aplicación.");

                return _cadenaConexion;
            }
        }

        /// <summary>
        /// Crea y devuelve un nuevo objeto SqlConnection usando la cadena de conexión configurada.
        /// El llamador es responsable de abrir y cerrar/liberar la conexión.
        /// </summary>
        public static SqlConnection CrearConexion()
        {
            return new SqlConnection(CadenaConexion);
        }
    }
}
