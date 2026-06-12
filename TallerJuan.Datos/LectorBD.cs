using Microsoft.Data.SqlClient;

namespace TallerJuan.Datos
{
    /// Utilitario de lectura segura de columnas de un SqlDataReader.
    /// Centraliza el manejo de nulos de la base de datos y tolera que una columna
    /// no exista en el resultado (devuelve un valor por defecto). Mismo espíritu que
    /// los métodos LeerTexto/LeerEntero usados en CD_Acceso y CD_Seguridad, reutilizable
    /// por todas las clases CD_ de la Fase 4.
    
    public static class LectorBD
    {
        /// Devuelve el texto de una columna, o cadena vacía si es NULL o no existe.
        public static string Texto(SqlDataReader lector, string columna)
        {
            int indice = Indice(lector, columna);
            if (indice < 0 || lector.IsDBNull(indice))
                return string.Empty;
            return lector.GetValue(indice)?.ToString() ?? string.Empty;
        }

        /// Devuelve el entero de una columna, o 0 si es NULL o no existe.
        public static int Entero(SqlDataReader lector, string columna)
        {
            int indice = Indice(lector, columna);
            if (indice < 0 || lector.IsDBNull(indice))
                return 0;
            return Convert.ToInt32(lector.GetValue(indice));
        }

        /// Devuelve el entero de una columna, o null si es NULL o no existe.
        public static int? EnteroNulo(SqlDataReader lector, string columna)
        {
            int indice = Indice(lector, columna);
            if (indice < 0 || lector.IsDBNull(indice))
                return null;
            return Convert.ToInt32(lector.GetValue(indice));
        }

        /// Devuelve el decimal de una columna, o 0 si es NULL o no existe.
        public static decimal Decimal(SqlDataReader lector, string columna)
        {
            int indice = Indice(lector, columna);
            if (indice < 0 || lector.IsDBNull(indice))
                return 0m;
            return Convert.ToDecimal(lector.GetValue(indice));
        }

        /// Devuelve el decimal de una columna, o null si es NULL o no existe.
        public static decimal? DecimalNulo(SqlDataReader lector, string columna)
        {
            int indice = Indice(lector, columna);
            if (indice < 0 || lector.IsDBNull(indice))
                return null;
            return Convert.ToDecimal(lector.GetValue(indice));
        }

        /// Devuelve la fecha de una columna, o DateTime.MinValue si es NULL o no existe.
        public static DateTime Fecha(SqlDataReader lector, string columna)
        {
            int indice = Indice(lector, columna);
            if (indice < 0 || lector.IsDBNull(indice))
                return DateTime.MinValue;
            return Convert.ToDateTime(lector.GetValue(indice));
        }

        /// Devuelve la fecha de una columna, o null si es NULL o no existe.
        public static DateTime? FechaNula(SqlDataReader lector, string columna)
        {
            int indice = Indice(lector, columna);
            if (indice < 0 || lector.IsDBNull(indice))
                return null;
            return Convert.ToDateTime(lector.GetValue(indice));
        }

        /// Obtiene el índice de una columna; devuelve -1 si no está en el resultado.
        private static int Indice(SqlDataReader lector, string columna)
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
