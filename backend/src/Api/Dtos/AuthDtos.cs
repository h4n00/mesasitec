namespace Api.Dtos;

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class UsuarioDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public string Rol { get; set; } = "";
    public Guid TenantId { get; set; }
    public string TenantNombre { get; set; } = "";
}

public class LoginResponse
{
    public string AccessToken { get; set; } = "";
    public int ExpiraEn { get; set; }
    public UsuarioDto Usuario { get; set; } = new();
}