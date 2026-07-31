using Dominio;
using Xunit;

namespace Pruebas;

public class PruebasCalculadoraSla
{
    // Fecha fija para que las pruebas no dependan del reloj
    private static readonly DateTime Base = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void PrioridadCritica_ReduceElPlazoALaMitad()
    {
        // Categoria de 8 horas con prioridad Critica (factor 0.5) = 4 horas
        var limite = CalculadoraSla.CalcularFechaLimite(Base, 8, Prioridad.Critica);

        Assert.Equal(Base.AddHours(4), limite);
    }

    [Fact]
    public void PrioridadBaja_DuplicaElPlazo()
    {
        // Categoria Consulta de 24 horas con prioridad Baja (factor 2.0) = 48 horas
        var limite = CalculadoraSla.CalcularFechaLimite(Base, 24, Prioridad.Baja);

        Assert.Equal(Base.AddHours(48), limite);
    }

    [Fact]
    public void SolicitudResuelta_NuncaEstaVencida()
    {
        var limitePasado = Base.AddHours(-10);
        var ahora = Base;

        var vencida = CalculadoraSla.EstaVencida(limitePasado, Estado.Resuelta, ahora);

        Assert.False(vencida);
    }

    [Fact]
    public void SolicitudNuevaConLimitePasado_EstaVencida()
    {
        var limitePasado = Base.AddHours(-1);
        var ahora = Base;

        var vencida = CalculadoraSla.EstaVencida(limitePasado, Estado.Nueva, ahora);

        Assert.True(vencida);
    }
}