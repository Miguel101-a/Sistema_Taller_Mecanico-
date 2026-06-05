using Microsoft.AspNetCore.Mvc;
using TallerJuan.Entidades;
using TallerJuan.Negocio;
using TallerJuan.Web.Models;
using TallerJuan.Web.Seguridad;

namespace TallerJuan.Web.Controllers
{
    /// <summary>
    /// Controlador de acceso al sistema: inicio y cierre de sesión.
    /// Marcado como anónimo para que el Login sea accesible sin sesión previa.
    /// </summary>
    [PermitirAnonimo]
    public class AccesoController : Controller
    {
        private readonly CN_Acceso _negocio = new CN_Acceso();

        /// <summary>
        /// Muestra el formulario de login. Si ya hay sesión activa, va directo al inicio.
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            if (SesionWeb.HaySesion(HttpContext.Session))
                return RedirectToAction("Index", "Inicio");

            return View(new LoginViewModel());
        }

        /// <summary>
        /// Procesa las credenciales. Si son válidas crea la sesión y va al inicio;
        /// si no, vuelve a mostrar el formulario con un mensaje de error.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            Empleado? empleado = _negocio.IniciarSesion(modelo.Usuario, modelo.Clave);

            if (empleado == null)
            {
                modelo.MensajeError = "Usuario o contraseña incorrectos.";
                modelo.Clave = string.Empty;
                return View(modelo);
            }

            // Se obtienen las claves de permiso del rol y se guarda todo en la sesión.
            List<string> permisos = _negocio.ObtenerClavesPermiso(empleado.IdRol);
            SesionWeb.IniciarSesion(HttpContext.Session, empleado, permisos);

            return RedirectToAction("Index", "Inicio");
        }

        /// <summary>
        /// Cierra la sesión: registra la auditoría, limpia la sesión y vuelve al Login.
        /// </summary>
        [HttpGet]
        public IActionResult CerrarSesion()
        {
            string usuario = SesionWeb.Usuario(HttpContext.Session);

            if (!string.IsNullOrEmpty(usuario))
                _negocio.RegistrarCierreSesion(usuario);

            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Acceso");
        }
    }
}
