using Api.Comun;
using Api.Dtos;
using Aplicacion;
using Infraestructura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly MesaSitecDbContext _db;
    private readonly GeneradorToken _generador;

    public AuthController(MesaSitecDbContext db, GeneradorToken generador)
    {
        _db = db;
        _generador = generador;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest peticion)
    {
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Email == peticion.Email && u.Activo);

        // Mismo error si el correo no existe o si la clave es incorrecta
        if (usuario == null ||
             !BCrypt.Net.BCrypt.Verify(peticion.Password, usuario.PasswordHash))
        {
            return Errores.NoAutenticado();
        }

        var tenant = await _db.Tenants.FirstAsync(t => t.Id == usuario.TenantId);

        var respuesta = new LoginResponse
        {
            AccessToken = _generador.Generar(usuario),
            ExpiraEn = _generador.ExpiraEnSegundos,
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol.ToString(),
                TenantId = usuario.TenantId,
                TenantNombre = tenant.Nombre
            }
        };

        return Ok(respuesta);
    }
    [HttpGet("/api/v1/me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        // El "sub" del token trae el id del usuario autenticado
        var idTexto = User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(idTexto, out var usuarioId))
            return Unauthorized();

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
        if (usuario == null)
            return Unauthorized();

        var tenant = await _db.Tenants.FirstAsync(t => t.Id == usuario.TenantId);

        return Ok(new UsuarioDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Rol = usuario.Rol.ToString(),
            TenantId = usuario.TenantId,
            TenantNombre = tenant.Nombre
        });
    }
}