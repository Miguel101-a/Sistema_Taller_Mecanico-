using System.ComponentModel.DataAnnotations;

namespace TallerJuan.Web.Models
{
    /// <summary>
    /// Modelo del formulario de inicio de sesión.
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingrese el usuario.")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese la contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Clave { get; set; } = string.Empty;

        /// <summary>Mensaje de error a mostrar cuando las credenciales son inválidas.</summary>
        public string? MensajeError { get; set; }
    }
}
