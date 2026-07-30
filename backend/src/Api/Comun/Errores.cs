using Microsoft.AspNetCore.Mvc;

namespace Api.Comun;

public static class Errores
{
    public static ObjectResult Crear(int status, string titulo, string codigo)
    {
        var problema = new ProblemDetails
        {
            Status = status,
            Title = titulo
        };
        problema.Extensions["codigo"] = codigo;

        return new ObjectResult(problema) { StatusCode = status };
    }

    public static ObjectResult NoAutenticado() =>
        Crear(401, "No autenticado", "NO_AUTENTICADO");

    public static ObjectResult NoPermitido() =>
        Crear(403, "Operacion no permitida", "OPERACION_NO_PERMITIDA");

    public static ObjectResult NoEncontrado() =>
        Crear(404, "Recurso no encontrado", "RECURSO_NO_ENCONTRADO");

    public static ObjectResult TransicionInvalida() =>
        Crear(409, "Transicion invalida", "TRANSICION_INVALIDA");

    public static ObjectResult AgenteInvalido() =>
        Crear(422, "Agente invalido", "AGENTE_INVALIDO");

    public static ObjectResult MotivoRequerido() =>
        Crear(422, "Motivo requerido", "MOTIVO_REQUERIDO");

    public static ObjectResult ParametroInvalido() =>
        Crear(400, "Parametro invalido", "PARAMETRO_INVALIDO");

    public static ObjectResult Validacion() =>
        Crear(422, "Error de validacion", "VALIDACION");
}