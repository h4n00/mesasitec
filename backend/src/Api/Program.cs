using Infraestructura;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Conexión a SQLite: archivo mesasitec.db en la carpeta del proyecto
builder.Services.AddDbContext<MesaSitecDbContext>(options =>
    options.UseSqlite("Data Source=mesasitec.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Crea la base de datos automáticamente si no existe
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MesaSitecDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// Endpoint de salud, sin autenticación (requisito del enunciado)
app.MapGet("/health", () => Results.Ok(new { estado = "ok" }));

app.Run();