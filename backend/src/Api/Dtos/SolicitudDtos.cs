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

public class CrearSolicitudRequest
{
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public Guid CategoriaId { get; set; }
    public Dominio.Prioridad Prioridad { get; set; }
}

public class EditarSolicitudRequest
{
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public Guid CategoriaId { get; set; }
    public Dominio.Prioridad Prioridad { get; set; }
}

public class SolicitudDetalleDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Estado { get; set; } = "";
    public string Prioridad { get; set; } = "";
    public ReferenciaDto Categoria { get; set; } = new();
    public ReferenciaDto Solicitante { get; set; } = new();
    public ReferenciaDto? Agente { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaLimiteSla { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public string? MotivoResolucion { get; set; }
    public string? MotivoCancelacion { get; set; }
    public bool Vencida { get; set; }
}

public class TransicionRequest
{
    public string Accion { get; set; } = "";
    public Guid? AgenteId { get; set; }
    public string? Motivo { get; set; }
}