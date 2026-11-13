using Buillon.Negocio;
using Buillon.Repositorio;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BuillonContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IRestauranteRepositorio, RestauranteRepositorio>();
builder.Services.AddScoped<AutenticacaoNegocio>();
builder.Services.AddScoped<RestauranteNegocio>();

// Diferença principal para a API: Usa Cookies (Navegador) em vez de JWT (Aplicativos Mobile)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

builder.Services.AddControllersWithViews(); // Habilita o suporte a HTML MVC

var app = builder.Build();

app.UseStaticFiles(); // Libera as pastas de CSS, Imagens e JavaScript
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Rota padrão do site (ex: abre Home/Index quando acessa a raiz do domínio)
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
