using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TallerJuan.Web.Seguridad
{
    /// <summary>
    /// Filtro de autorización global. Antes de ejecutar cualquier acción comprueba que exista
    /// una sesión iniciada. Si no la hay, redirige al Login. Las acciones o controladores
    /// marcados con <see cref="PermitirAnonimoAttribute"/> (como el propio Login) quedan exentos.
    /// Los archivos estáticos no pasan por este filtro, por lo que siguen accesibles.
    /// </summary>
    public sealed class FiltroSesion : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Si la acción o el controlador permiten acceso anónimo, no se exige sesión.
            bool permiteAnonimo = context.ActionDescriptor.EndpointMetadata
                .OfType<PermitirAnonimoAttribute>()
                .Any();

            if (permiteAnonimo)
                return;

            // Si no hay usuario en sesión, se redirige a la pantalla de Login.
            if (!SesionWeb.HaySesion(context.HttpContext.Session))
            {
                context.Result = new RedirectToActionResult("Login", "Acceso", null);
            }
        }
    }
}
