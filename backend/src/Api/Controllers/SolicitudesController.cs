using Api.Comun;
using Api.Dtos;
using Dominio;
using Infraestructura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/solicitudes")]
[Authorize]
public class SolicitudesController : ControllerBase
{
    private readonly MesaSitecDbContext _db;

    public SolicitudesController(MesaSitecDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Estado? estado,
        [FromQuery] Prioridad? prioridad,
        [FromQuery] Guid? categoriaId,
        [FromQuery] Guid? agenteId,
        [FromQuery] string? q,
        [FromQuery] bool? vencidas,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sort = "-fechaCreacion")
    {
        if (page < 1 || pageSize > 100 || pageSize < 1)
            return Errores.ParametroInvalido();

        var actual = new UsuarioActual(User);
        var ahora = DateTime.UtcNow;

        // Base: solo la organizacion del token (RN-01)
        var consulta = _db.Solicitudes
            .Where(s => s.TenantId == actual.TenantId);

        // El solicitante solo ve las suyas (RN-03)
        if (!actual.VeTodasLasSolicitudes)
            consulta = consulta.Where(s => s.SolicitanteId == actual.Id);

        // Filtros exactos
        if (estado.HasValue)
            consulta = consulta.Where(s => s.Estado == estado.Value);

        if (prioridad.HasValue)
            consulta = consulta.Where(s => s.Prioridad == prioridad.Value);

        if (categoriaId.HasValue)
            consulta = consulta.Where(s => s.CategoriaId == categoriaId.Value);

        if (agenteId.HasValue)
            consulta = consulta.Where(s => s.AgenteId == agenteId.Value);

        // Busqueda sin distinguir mayusculas
        if (!string.IsNullOrWhiteSpace(q))
        {
            var texto = q.ToLower();
            consulta = consulta.Where(s =>
                s.Titulo.ToLower().Contains(texto) ||
                s.Descripcion.ToLower().Contains(texto) ||
                s.Codigo.ToLower().Contains(texto));
        }

        // Vencidas segun RN-04
        if (vencidas.HasValue)
        {
            if (vencidas.Value)
                consulta = consulta.Where(s =>
                    s.FechaLimiteSla < ahora &&
                    s.Estado != Estado.Resuelta &&
                    s.Estado != Estado.Cerrada &&
                    s.Estado != Estado.Cancelada);
            else
                consulta = consulta.Where(s =>
                    s.FechaLimiteSla >= ahora ||
                    s.Estado == Estado.Resuelta ||
                    s.Estado == Estado.Cerrada ||
                    s.Estado == Estado.Cancelada);
        }

        consulta = Ordenar(consulta, sort);

        var total = await consulta.CountAsync();

        var solicitudes = await consulta
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Se cargan categorias y agentes para armar las referencias
        var categorias = await _db.Categorias
            .Where(c => c.TenantId == actual.TenantId)
            .ToListAsync();

        var usuarios = await _db.Usuarios
            .Where(u => u.TenantId == actual.TenantId)
            .ToListAsync();

        var items = solicitudes.Select(s => new SolicitudListaDto
        {
            Id = s.Id,
            Codigo = s.Codigo,
            Titulo = s.Titulo,
            Estado = s.Estado.ToString(),
            Prioridad = s.Prioridad.ToString(),
            Categoria = categorias
                .Where(c => c.Id == s.CategoriaId)
                .Select(c => new ReferenciaDto { Id = c.Id, Nombre = c.Nombre })
                .FirstOrDefault() ?? new ReferenciaDto(),
            Agente = usuarios
                .Where(u => u.Id == s.AgenteId)
                .Select(u => new ReferenciaDto { Id = u.Id, Nombre = u.Nombre })
                .FirstOrDefault(),
            FechaCreacion = DateTime.SpecifyKind(s.FechaCreacion, DateTimeKind.Utc),
            FechaLimiteSla = DateTime.SpecifyKind(s.FechaLimiteSla, DateTimeKind.Utc),
            Vencida = CalculadoraSla.EstaVencida(s.FechaLimiteSla, s.Estado, ahora)
        }).ToList();

        var respuesta = new PaginaDto<SolicitudListaDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)pageSize)
        };

        return Ok(respuesta);
    }



    [HttpGet("{id}")]
    public async Task<IActionResult> Detalle(Guid id)
    {
        var actual = new UsuarioActual(User);
        var ahora = DateTime.UtcNow;

        var s = await _db.Solicitudes
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == actual.TenantId);

        // RN-01: si no existe o es de otra organizacion, 404
        if (s == null)
            return Errores.NoEncontrado();

        // RN-03: el solicitante solo ve las propias (tambien 404, no 403)
        if (!actual.VeTodasLasSolicitudes && s.SolicitanteId != actual.Id)
            return Errores.NoEncontrado();

        var dto = await ArmarDetalle(s, actual.TenantId, ahora);
        return Ok(dto);
    }

    private async Task<SolicitudDetalleDto> ArmarDetalle(Solicitud s, Guid tenantId, DateTime ahora)
    {
        var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == s.CategoriaId);
        var solicitante = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == s.SolicitanteId);
        var agente = s.AgenteId.HasValue
            ? await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == s.AgenteId.Value)
            : null;

        return new SolicitudDetalleDto
        {
            Id = s.Id,
            Codigo = s.Codigo,
            Titulo = s.Titulo,
            Descripcion = s.Descripcion,
            Estado = s.Estado.ToString(),
            Prioridad = s.Prioridad.ToString(),
            Categoria = categoria == null
                ? new ReferenciaDto()
                : new ReferenciaDto { Id = categoria.Id, Nombre = categoria.Nombre },
            Solicitante = solicitante == null
                ? new ReferenciaDto()
                : new ReferenciaDto { Id = solicitante.Id, Nombre = solicitante.Nombre },
            Agente = agente == null
                ? null
                : new ReferenciaDto { Id = agente.Id, Nombre = agente.Nombre },
            FechaCreacion = DateTime.SpecifyKind(s.FechaCreacion, DateTimeKind.Utc),
            FechaLimiteSla = DateTime.SpecifyKind(s.FechaLimiteSla, DateTimeKind.Utc),
            FechaResolucion = s.FechaResolucion.HasValue
                ? DateTime.SpecifyKind(s.FechaResolucion.Value, DateTimeKind.Utc)
                : null,
            MotivoResolucion = s.MotivoResolucion,
            MotivoCancelacion = s.MotivoCancelacion,
            Vencida = CalculadoraSla.EstaVencida(s.FechaLimiteSla, s.Estado, ahora)
        };
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearSolicitudRequest peticion)
    {
        var actual = new UsuarioActual(User);
        var ahora = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(peticion.Titulo) ||
            string.IsNullOrWhiteSpace(peticion.Descripcion))
            return Errores.Validacion();

        // La categoria debe existir, estar activa y ser de la misma organizacion
        var categoria = await _db.Categorias.FirstOrDefaultAsync(c =>
            c.Id == peticion.CategoriaId &&
            c.TenantId == actual.TenantId &&
            c.Activo);

        if (categoria == null)
            return Errores.Validacion();

        var solicitud = new Solicitud
        {
            Id = Guid.NewGuid(),
            TenantId = actual.TenantId,
            Codigo = await GenerarCodigo(actual.TenantId, ahora.Year),
            Titulo = peticion.Titulo,
            Descripcion = peticion.Descripcion,
            CategoriaId = categoria.Id,
            Prioridad = peticion.Prioridad,
            Estado = Estado.Nueva,
            SolicitanteId = actual.Id,
            FechaCreacion = ahora,
            FechaLimiteSla = CalculadoraSla.CalcularFechaLimite(
                ahora, categoria.SlaHoras, peticion.Prioridad)
        };

        _db.Solicitudes.Add(solicitud);
        await _db.SaveChangesAsync();

        var dto = await ArmarDetalle(solicitud, actual.TenantId, ahora);

        return CreatedAtAction(nameof(Detalle), new { id = solicitud.Id }, dto);
    }

    // RN-07: correlativo independiente por organizacion y por año
    private async Task<string> GenerarCodigo(Guid tenantId, int anio)
    {
        var prefijo = $"SOL-{anio}-";

        var cantidad = await _db.Solicitudes
            .CountAsync(s => s.TenantId == tenantId && s.Codigo.StartsWith(prefijo));

        var siguiente = cantidad + 1;
        return prefijo + siguiente.ToString("D5");
    }

    private static IQueryable<Solicitud> Ordenar(IQueryable<Solicitud> consulta, string sort)
    {
        return sort switch
        {
            "fechaCreacion" => consulta.OrderBy(s => s.FechaCreacion),
            "-fechaCreacion" => consulta.OrderByDescending(s => s.FechaCreacion),
            "prioridad" => consulta.OrderBy(s => s.Prioridad),
            "-prioridad" => consulta.OrderByDescending(s => s.Prioridad),
            "codigo" => consulta.OrderBy(s => s.Codigo),
            _ => consulta.OrderByDescending(s => s.FechaCreacion)
        };
    }
}