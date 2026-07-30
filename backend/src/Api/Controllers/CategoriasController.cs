using Api.Comun;
using Api.Dtos;
using Infraestructura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/categorias")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly MesaSitecDbContext _db;

    public CategoriasController(MesaSitecDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var actual = new UsuarioActual(User);

        var categorias = await _db.Categorias
            .Where(c => c.TenantId == actual.TenantId && c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                SlaHoras = c.SlaHoras
            })
            .ToListAsync();

        return Ok(categorias);
    }
}