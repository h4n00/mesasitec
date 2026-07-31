using Dominio;
using Xunit;

namespace Pruebas;

public class PruebasMaquinaEstados
{
    [Fact]
    public void DesdeNueva_AsignarLlevaAAsignada()
    {
        var destino = MaquinaEstados.ObtenerDestino(Estado.Nueva, "asignar");

        Assert.Equal(Estado.Asignada, destino);
    }

    [Fact]
    public void DesdeNueva_ResolverEsInvalido()
    {
        // No se puede resolver algo que nadie ha tomado
        var destino = MaquinaEstados.ObtenerDestino(Estado.Nueva, "resolver");

        Assert.Null(destino);
    }

    [Fact]
    public void EstadoCerrada_NoAdmiteNingunaAccion()
    {
        var acciones = new[] { "asignar", "iniciar", "resolver", "cerrar", "reabrir", "cancelar" };

        foreach (var accion in acciones)
        {
            var destino = MaquinaEstados.ObtenerDestino(Estado.Cerrada, accion);
            Assert.Null(destino);
        }
    }

    [Fact]
    public void DesdeAsignada_AsignarPermiteReasignar()
    {
        // Reasignar a otro agente mantiene el mismo estado
        var destino = MaquinaEstados.ObtenerDestino(Estado.Asignada, "asignar");

        Assert.Equal(Estado.Asignada, destino);
    }
}