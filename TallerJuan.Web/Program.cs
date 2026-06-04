using TallerJuan.Datos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
