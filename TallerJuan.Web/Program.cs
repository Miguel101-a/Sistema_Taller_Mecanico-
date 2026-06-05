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
    opciones.IdleTimeout = TimeSpan.FromMinutes(30);
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
});

// Se lee la cadena de conexión "TallerJuanDB" desde appsettings.json
// y se entrega a la capa de Datos UNA sola vez al iniciar la aplicación.
var cadenaConexion = builder.Configuration.GetConnectionString("TallerJuanDB")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'TallerJuanDB' en la configuración.");
ConexionBD.Configurar(cadenaConexion);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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
