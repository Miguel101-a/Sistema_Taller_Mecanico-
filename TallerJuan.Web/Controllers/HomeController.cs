using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TallerJuan.Web.Models;
using TallerJuan.Web.Seguridad;

namespace TallerJuan.Web.Controllers;

// Páginas de plantilla y manejador de errores: accesibles sin sesión para evitar
// redirecciones en bucle cuando ocurre un error antes de autenticarse.
[PermitirAnonimo]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Página amigable para códigos de estado HTTP (404, etc.). El código llega por querystring
    // desde UseStatusCodePagesWithReExecute; se conserva el código de estado en la respuesta.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult EstadoHttp(int codigo)
    {
        Response.StatusCode = codigo;
        ViewData["Codigo"] = codigo;
        return View();
    }
}
