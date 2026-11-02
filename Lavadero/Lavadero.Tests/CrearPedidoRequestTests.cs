using System.ComponentModel.DataAnnotations;
using Lavadero.Application.Models;
using Xunit;

namespace Lavadero.Tests;

public sealed class CrearPedidoRequestTests
{
    [Fact]
    public void CamposVaciosDevuelvenMensajesEnEspanol()
    {
        var request = new CrearPedidoRequest();
        var resultados = new List<ValidationResult>();

        var valido = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            resultados,
            validateAllProperties: true);

        Assert.False(valido);
        Assert.Contains(resultados, x => x.ErrorMessage == "El nombre es obligatorio.");
        Assert.Contains(resultados, x => x.ErrorMessage == "El apellido es obligatorio.");
        Assert.Contains(resultados, x => x.ErrorMessage == "El celular es obligatorio.");
    }

    [Fact]
    public void CelularCortoDevuelveLaReglaCorrecta()
    {
        var request = new CrearPedidoRequest
        {
            Nombre = "Ana",
            Apellido = "Pérez",
            Celular = "123"
        };
        var resultados = new List<ValidationResult>();

        var valido = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            resultados,
            validateAllProperties: true);

        Assert.False(valido);
        Assert.Contains(resultados, x =>
            x.ErrorMessage == "El celular debe tener entre 6 y 30 caracteres.");
    }

    [Fact]
    public void PedidoSinProductosEsInvalido()
    {
        var request = CrearRequestValido();

        var resultados = Validar(request);

        Assert.Contains(resultados, x =>
            x.ErrorMessage == "El pedido debe incluir entre 1 y 50 productos diferentes.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void CantidadFueraDelLimiteEsInvalida(int cantidad)
    {
        var request = CrearRequestValido();
        request.Cantidades[1] = cantidad;

        var resultados = Validar(request);

        Assert.Contains(resultados, x =>
            x.ErrorMessage == "La cantidad de cada producto debe estar entre 1 y 100.");
    }

    [Fact]
    public void PedidoCompletoEsValido()
    {
        var request = CrearRequestValido();
        request.Cantidades[1] = 2;

        Assert.Empty(Validar(request));
    }

    private static CrearPedidoRequest CrearRequestValido() => new()
    {
        Nombre = "Ana",
        Apellido = "Pérez",
        Celular = "1122334455"
    };

    private static List<ValidationResult> Validar(CrearPedidoRequest request)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            resultados,
            validateAllProperties: true);
        return resultados;
    }
}
