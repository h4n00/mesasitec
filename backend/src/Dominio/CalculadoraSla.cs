namespace Dominio;

public static class CalculadoraSla
{
    // Cada prioridad ajusta el plazo base de la categoría
    public static double ObtenerFactor(Prioridad prioridad)
    {
        return prioridad switch
        {
            Prioridad.Critica => 0.5,
            Prioridad.Alta => 0.75,
            Prioridad.Media => 1.0,
            Prioridad.Baja => 2.0,
            _ => 1.0
        };
    }

    public static DateTime CalcularFechaLimite(DateTime fechaCreacion, int slaHoras, Prioridad prioridad)
    {
        var horas = slaHoras * ObtenerFactor(prioridad);
        return fechaCreacion.AddHours(horas);
    }

    public static bool EstaVencida(DateTime fechaLimiteSla, Estado estado, DateTime ahora)
    {
        if (estado == Estado.Resuelta || estado == Estado.Cerrada || estado == Estado.Cancelada)
            return false;

        return fechaLimiteSla < ahora;
    }
}