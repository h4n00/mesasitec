using Dominio;

namespace Infraestructura;

public static class SembradorDatos
{
    public static void Sembrar(MesaSitecDbContext db)
    {
        // Si ya hay datos, no hace nada
        if (db.Tenants.Any()) return;

        var fechaBase = ObtenerFechaBase();

        // 1. Organizaciones
        var norte = new Tenant { Id = Guid.NewGuid(), Nombre = "Cooperativa Norte", Activo = true };
        var sur = new Tenant { Id = Guid.NewGuid(), Nombre = "Bufete Sur", Activo = true };
        db.Tenants.AddRange(norte, sur);

        // 2. Usuarios (misma contraseña para todos)
        var hash = BCrypt.Net.BCrypt.HashPassword("Sitec.2026");

        var usuarios = new List<Usuario>
        {
            CrearUsuario(norte.Id, "admin@norte.test",  "Ana Admin",     Rol.Admin,       hash),
            CrearUsuario(norte.Id, "agente1@norte.test","Carlos Agente", Rol.Agente,      hash),
            CrearUsuario(norte.Id, "agente2@norte.test","Diana Agente",  Rol.Agente,      hash),
            CrearUsuario(norte.Id, "user1@norte.test",  "Luis Usuario",  Rol.Solicitante, hash),
            CrearUsuario(norte.Id, "user2@norte.test",  "Marta Usuario", Rol.Solicitante, hash),
            CrearUsuario(sur.Id,   "admin@sur.test",    "Pedro Admin",   Rol.Admin,       hash),
            CrearUsuario(sur.Id,   "user1@sur.test",    "Sofia Usuario", Rol.Solicitante, hash)
        };
        db.Usuarios.AddRange(usuarios);

        // 3. Categorías (las mismas 4 en cada organización)
        var categorias = new List<Categoria>();
        foreach (var tenantId in new[] { norte.Id, sur.Id })
        {
            categorias.Add(CrearCategoria(tenantId, "Incidente", 8));
            categorias.Add(CrearCategoria(tenantId, "Requerimiento", 40));
            categorias.Add(CrearCategoria(tenantId, "Consulta", 24));
            categorias.Add(CrearCategoria(tenantId, "Falla crítica", 4));
        }
        db.Categorias.AddRange(categorias);

        db.SaveChanges();
    }

    private static DateTime ObtenerFechaBase()
    {
        var valor = Environment.GetEnvironmentVariable("SEED_FECHA_BASE");
        if (string.IsNullOrWhiteSpace(valor))
            valor = "2026-01-15T08:00:00Z";

        return DateTime.Parse(valor).ToUniversalTime();
    }

    private static Usuario CrearUsuario(Guid tenantId, string email, string nombre, Rol rol, string hash)
    {
        return new Usuario
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            Nombre = nombre,
            Rol = rol,
            PasswordHash = hash,
            Activo = true
        };
    }

    private static Categoria CrearCategoria(Guid tenantId, string nombre, int slaHoras)
    {
        return new Categoria
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Nombre = nombre,
            SlaHoras = slaHoras,
            Activo = true
        };
    }
}