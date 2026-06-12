using TallerJuan.Datos;
using TallerJuan.Web.Seguridad;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Se registra el filtro de sesión como filtro GLOBAL: toda acción exige sesión iniciada,
// salvo las marcadas con [PermitirAnonimo] (como el Login).
builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add<FiltroSesion>();
});

// Se habilita el manejo de sesión en memoria del servidor.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{
    // RNF-04: la sesión se cierra automáticamente tras 15 minutos de inactividad.
    opciones.IdleTimeout = TimeSpan.FromMinutes(15);
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
});

// Se lee la cadena de conexión "TallerJuanDB" desde appsettings.json
// y se entrega a la capa de Datos UNA sola vez al iniciar la aplicación.
var cadenaConexion = builder.Configuration.GetConnectionString("TallerJuanDB")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'TallerJuanDB' en la configuración.");
ConexionBD.Configurar(cadenaConexion);

var app = builder.Build();

// Configuración del flujo de peticiones HTTP.
if (!app.Environment.IsDevelopment())
{
    // En producción, cualquier excepción no controlada se desvía a la página de error amigable.
    app.UseExceptionHandler("/Home/Error");
    // El valor por defecto de HSTS es 30 días; ajústelo para escenarios de producción: https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Respuestas amigables para códigos de estado (p. ej. 404): se re-ejecuta la acción
// Home/EstadoHttp pasándole el código, conservando la estética del sistema.
app.UseStatusCodePagesWithReExecute("/Home/EstadoHttp", "?codigo={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// La sesión debe activarse antes de la autorización para que el filtro pueda leerla.
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inicio}/{action=Index}/{id?}");

app.Run();
