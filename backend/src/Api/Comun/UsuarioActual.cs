using System.Security.Claims;
using Dominio;

namespace Api.Comun;

public class UsuarioActual
{
    public Guid Id { get; }
    public Guid TenantId { get; }
    public Rol Rol { get; }

    public UsuarioActual(ClaimsPrincipal usuario)
    {
        Id = Guid.Parse(usuario.FindFirst("sub")!.Value);
        TenantId = Guid.Parse(usuario.FindFirst("tenantId")!.Value);
        Rol = Enum.Parse<Rol>(usuario.FindFirst("rol")!.Value);
    }

    public bool EsAdmin => Rol == Rol.Admin;
    public bool EsAgente => Rol == Rol.Agente;
    public bool EsSolicitante => Rol == Rol.Solicitante;

    // Admin y Agente ven todo lo de su organizacion; el Solicitante solo lo suyo
    public bool VeTodasLasSolicitudes => Rol == Rol.Admin || Rol == Rol.Agente;
}