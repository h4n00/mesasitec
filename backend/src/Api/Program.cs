using Infraestructura;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Aplicacion;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Conexión a SQLite: archivo mesasitec.db en la carpeta del proyecto
builder.Services.AddDbContext<MesaSitecDbContext>(options =>
    options.UseSqlite("Data Source=mesasitec.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var esquema = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Pega aqui el accessToken (sin escribir Bearer)",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", esquema);

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { esquema, new List<string>() }
    });
});

// Configuracion del JWT leida desde appsettings.json
var jwtSecreto = builder.Configuration["Jwt:Secreto"]!;
var jwtEmisor = builder.Configuration["Jwt:Emisor"]!;
var jwtAudiencia = builder.Configuration["Jwt:Audiencia"]!;
var jwtExpira = int.Parse(builder.Configuration["Jwt:ExpiraEnSegundos"]!);

builder.Services.AddSingleton(new GeneradorToken(
    jwtSecreto, jwtEmisor, jwtAudiencia, jwtExpira));

// Le enseña a la API a validar el token que llega en cada peticion
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtEmisor,
            ValidAudience = jwtAudiencia,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecreto))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Crea la base de datos automáticamente si no existe
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MesaSitecDbContext>();
    db.Database.EnsureCreated();
    SembradorDatos.Sembrar(db);
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint de salud, sin autenticación (requisito del enunciado)
app.MapGet("/health", () => Results.Ok(new { estado = "ok" }));



app.Run();