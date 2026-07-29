namespace Dominio;

public class Usuario
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Nombre { get; set; } = "";
    public Rol Rol { get; set; }
    public bool Activo { get; set; } = true;
}