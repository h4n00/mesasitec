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