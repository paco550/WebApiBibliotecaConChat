using Microsoft.EntityFrameworkCore;
using WebApiBiblioteca.Services;
using WebApiBiblioteca.Models;
using WebApiBiblioteca.Middlewares;
using System.Text.Json.Serialization;
using WebApiBiblioteca.Filters;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using WebApiBiblioteca.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    // Integramos el filtro de excepción para todos los controladores
    options.Filters.Add<FiltroDeExcepcion>();
}).AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Capturamos del app.settings la cadena de conexión a la base de datos
// Configuration.GetConnectionString va directamente a la propiedad ConnectionStrings y de ahí tomamos el valor de DefaultConnection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Nuestros servicios resolverán dependencias de otras clases
// Registramos en el sistema de inyección de dependencias de la aplicación el ApplicationDbContext
// Conseguimos una instancia o configuración global de la base de datos para todo el proyecto
builder.Services.AddDbContext<MiBibliotecaContext>(options =>
{
    options.UseSqlServer(connectionString);
    // Esta opción deshabilita el tracking a nivel de proyecto (NoTracking).
    // Por defecto siempre hace el tracking. Con esta configuración, no.
    // En cada operación de modificación de datos en los controladores, deberemos habilitar el tracking en cada operación
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}
);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IGestorArchivos, GestorArchivosLocal>();
builder.Services.AddTransient<OperacionesService>();
builder.Services.AddTransient<HashService>();
builder.Services.AddTransient<TokenService>();

// Para el chat en tiempo real
builder.Services.AddSignalR();

// builder.Services.AddHostedService<TareaProgramadaService>();
builder.Services.AddDataProtection();

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = false,
                   ValidateAudience = false,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(
                     Encoding.UTF8.GetBytes(builder.Configuration["ClaveJWT"]))
               });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors();

app.UseMiddleware<LogFileIPMiddleware>();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chatHub"); // El acceso al hub sería vía https://localhost:puerto/chatHub
});

app.UseAuthorization();

app.MapControllers();

app.Run();
