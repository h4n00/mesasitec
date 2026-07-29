using Api.Dtos;
using Aplicacion;
using Infraestructura;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var problema = new ProblemDetails
            {
                Status = 401,
                Title = "Credenciales invalidas"
            };
            problema.Extensions["codigo"] = "NO_AUTENTICADO";

            return new ObjectResult(problema) { StatusCode = 401 };
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
}