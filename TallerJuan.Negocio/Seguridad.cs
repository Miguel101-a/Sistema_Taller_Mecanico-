using System.Security.Cryptography;
using System.Text;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Clase utilitaria de seguridad. Contiene el cálculo de hash SHA-256
    /// compatible con el que genera SQL Server mediante HASHBYTES('SHA2_256', ...).
    /// </summary>
    public static class Seguridad
    {
        /// <summary>
        /// Calcula el hash SHA-256 de un texto, SIN sal (salt), sobre sus bytes UTF-8,
        /// y devuelve el resultado en hexadecimal en MAYÚSCULAS (64 caracteres).
        ///
        /// Equivale a la siguiente expresión en SQL Server:
        ///   UPPER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(VARBINARY(MAX), @texto)), 2))
        /// </summary>
        public static string HashSHA256(string texto)
        {
            // Se toman los bytes UTF-8 del texto de entrada (sin sal).
            byte[] bytesEntrada = Encoding.UTF8.GetBytes(texto ?? string.Empty);

            // Se calcula el hash SHA-256.
            byte[] bytesHash = SHA256.HashData(bytesEntrada);

            // Se convierte a hexadecimal en MAYÚSCULAS, sin guiones ni espacios.
            return Convert.ToHexString(bytesHash); // ToHexString ya devuelve en MAYÚSCULAS
        }
    }
}
