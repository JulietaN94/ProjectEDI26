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
}
