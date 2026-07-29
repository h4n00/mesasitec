using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dominio;
using Microsoft.IdentityModel.Tokens;

namespace Aplicacion;

public class GeneradorToken
{
    private readonly string _secreto;
    private readonly string _emisor;
    private readonly string _audiencia;
    private readonly int _expiraEnSegundos;

    public GeneradorToken(string secreto, string emisor, string audiencia, int expiraEnSegundos)
    {
        _secreto = secreto;
        _emisor = emisor;
        _audiencia = audiencia;
        _expiraEnSegundos = expiraEnSegundos;
    }

    public int ExpiraEnSegundos => _expiraEnSegundos;

    public string Generar(Usuario usuario)
    {
        // Los claims son los datos que viajan dentro del token
        var claims = new List<Claim>
        {
            new Claim("sub", usuario.Id.ToString()),
            new Claim("email", usuario.Email),
            new Claim("rol", usuario.Rol.ToString()),
            new Claim("tenantId", usuario.TenantId.ToString())
        };

        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secreto));
        var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _emisor,
            audience: _audiencia,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(_expiraEnSegundos),
            signingCredentials: credenciales
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}