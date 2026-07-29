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

        var anio = fechaBase.Year;

        var catsNorte = categorias.Where(c => c.TenantId == norte.Id).ToList();
        var usrNorte = usuarios.Where(u => u.TenantId == norte.Id).ToList();
        SembrarSolicitudes(db, norte.Id, catsNorte, usrNorte, fechaBase, 25, anio);

        var catsSur = categorias.Where(c => c.TenantId == sur.Id).ToList();
        var usrSur = usuarios.Where(u => u.TenantId == sur.Id).ToList();
        SembrarSolicitudes(db, sur.Id, catsSur, usrSur, fechaBase, 8, anio);

        db.SaveChanges();
    }

    private static void SembrarSolicitudes(
        MesaSitecDbContext db,
        Guid tenantId,
        List<Categoria> categorias,
        List<Usuario> usuarios,
        DateTime fechaBase,
        int cantidad,
        int anio)
    {
        var solicitantes = usuarios.Where(u => u.Rol == Rol.Solicitante).ToList();
        var agentes = usuarios.Where(u => u.Rol == Rol.Agente || u.Rol == Rol.Admin).ToList();

        var estados = new[]
        {
            Estado.Nueva, Estado.Asignada, Estado.EnProceso,
            Estado.Resuelta, Estado.Cerrada, Estado.Cancelada
        };
        var prioridades = new[]
        {
            Prioridad.Baja, Prioridad.Media, Prioridad.Alta, Prioridad.Critica
        };

        var solicitudes = new List<Solicitud>();

        for (int i = 0; i < cantidad; i++)
        {
            var categoria = categorias[i % categorias.Count];
            var prioridad = prioridades[i % prioridades.Length];
            var estado = estados[i % estados.Length];
            var solicitante = solicitantes[i % solicitantes.Count];

            // Las primeras 8 se crean muy atrás en el tiempo para que venzan
            var horasAtras = i < 8 ? 240 : 2;
            var fechaCreacion = fechaBase.AddHours(-horasAtras).AddMinutes(i);

            var solicitud = new Solicitud
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Codigo = $"SOL-{anio}-{(i + 1).ToString("D5")}",
                Titulo = $"Solicitud de prueba {i + 1}",
                Descripcion = $"Descripcion detallada de la solicitud numero {i + 1} generada por la semilla.",
                CategoriaId = categoria.Id,
                Prioridad = prioridad,
                Estado = estado,
                SolicitanteId = solicitante.Id,
                FechaCreacion = fechaCreacion,
                FechaLimiteSla = CalculadoraSla.CalcularFechaLimite(
                    fechaCreacion, categoria.SlaHoras, prioridad)
            };

            // Los estados que ya pasaron por asignacion necesitan agente
            if (estado != Estado.Nueva && estado != Estado.Cancelada)
                solicitud.AgenteId = agentes[i % agentes.Count].Id;

            if (estado == Estado.Resuelta || estado == Estado.Cerrada)
            {
                solicitud.FechaResolucion = fechaCreacion.AddHours(1);
                solicitud.MotivoResolucion = "Se atendio la solicitud y se valido con el usuario final.";
            }

            if (estado == Estado.Cancelada)
                solicitud.MotivoCancelacion = "Solicitud duplicada.";

            solicitudes.Add(solicitud);
        }

        db.Solicitudes.AddRange(solicitudes);
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