namespace TallerJuan.Web.Seguridad
{
    /// <summary>
    /// Marca un controlador o acción como accesible SIN sesión iniciada
    /// (por ejemplo, la pantalla de Login). El filtro de sesión omite la
    /// verificación cuando encuentra este atributo.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class PermitirAnonimoAttribute : Attribute
    {
    }
}
