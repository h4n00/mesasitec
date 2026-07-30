namespace Api.Dtos;

public class CategoriaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public int SlaHoras { get; set; }
}