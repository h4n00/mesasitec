namespace Dominio;

public static class MaquinaEstados
{
    // Devuelve el estado destino si la acción es válida; null si no lo es
    public static Estado? ObtenerDestino(Estado estadoActual, string accion)
    {
        return (estadoActual, accion) switch
        {
            (Estado.Nueva, "asignar") => Estado.Asignada,
            (Estado.Nueva, "cancelar") => Estado.Cancelada,

            (Estado.Asignada, "iniciar") => Estado.EnProceso,
            (Estado.Asignada, "asignar") => Estado.Asignada,
            (Estado.Asignada, "cancelar") => Estado.Cancelada,

            (Estado.EnProceso, "resolver") => Estado.Resuelta,
            (Estado.EnProceso, "asignar") => Estado.Asignada,
            (Estado.EnProceso, "cancelar") => Estado.Cancelada,

            (Estado.Resuelta, "cerrar") => Estado.Cerrada,
            (Estado.Resuelta, "reabrir") => Estado.EnProceso,

            // Cerrada y Cancelada son finales: no admiten ninguna acción
            _ => null
        };
    }
}