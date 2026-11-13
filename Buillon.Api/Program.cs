using System.Text;
using Buillon.Negocio;
using Buillon.Repositorio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configura o Entity Framework com o SQL Server
builder.Services.AddDbContext<BuillonContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Injeção de Dependências: Ensina o sistema quem chamar quando precisarmos de Repositorios e Negocios
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IRestauranteRepositorio, RestauranteRepositorio>();
builder.Services.AddScoped<AutenticacaoNegocio>();
builder.Services.AddScoped<RestauranteNegocio>();

// CORS: Permite que o site frontend consuma esta API sem bloqueios de segurança do navegador
builder.Services.AddCors(opcoes => opcoes.AddDefaultPolicy(p => p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

// Configura o sistema para validar o Token JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = false, ValidateAudience = false, ValidateLifetime = true, ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "ChaveSecretaBuillonDesenvolvimentoComMaisDe32Caracteres!"))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // Documentação automática (Swagger)

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.MapOpenApi(); }

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication(); // Liga o porteiro (Verifica quem é)
app.UseAuthorization(); // Liga o segurança (Verifica o que pode acessar)

app.MapControllers(); // Habilita os endpoints de Auth e Restaurantes
app.Run();
