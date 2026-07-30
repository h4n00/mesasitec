namespace Api.Dtos;

public class ReferenciaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class SolicitudListaDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Estado { get; set; } = "";
    public string Prioridad { get; set; } = "";
    public ReferenciaDto Categoria { get; set; } = new();
    public ReferenciaDto? Agente { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaLimiteSla { get; set; }
    public bool Vencida { get; set; }
}

public class PaginaDto<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPaginas { get; set; }
}