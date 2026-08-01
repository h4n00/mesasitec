using Api.Comun;
using Api.Dtos;
using Dominio;
using Infraestructura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly MesaSitecDbContext _db;

    public UsuariosController(MesaSitecDbContext db)
    {
        _db = db;
    }

    // Endpoint adicional al contrato: el selector de agentes del modal
    // de asignacion necesita la lista de agentes del tenant.
    [HttpGet("agentes")]
    public async Task<IActionResult> Agentes()
    {
        var actual = new UsuarioActual(User);

        var agentes = await _db.Usuarios
            .Where(u => u.TenantId == actual.TenantId &&
                        u.Activo &&
                        (u.Rol == Rol.Agente || u.Rol == Rol.Admin))
            .OrderBy(u => u.Nombre)
            .Select(u => new ReferenciaDto { Id = u.Id, Nombre = u.Nombre })
            .ToListAsync();

        return Ok(agentes);
    }
}