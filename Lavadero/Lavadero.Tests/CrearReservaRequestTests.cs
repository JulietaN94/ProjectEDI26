using System.ComponentModel.DataAnnotations;
using Lavadero.Application.Models;
using Xunit;

namespace Lavadero.Tests;

public sealed class CrearReservaRequestTests
{
    [Fact]
    public void TelefonoConLetrasEsInvalido()
    {
        var request = CrearValido();
        request.Telefono = "11ABC123";

        var resultados = Validar(request);

        Assert.Contains(resultados, resultado =>
            resultado.ErrorMessage ==
            "El teléfono solo puede contener números, espacios y los símbolos +, -, ( y ).");
    }

    [Fact]
    public void ServicioDebeEstarSeleccionado()
    {
        var request = CrearValido();
        request.ServicioId = 0;

        Assert.Contains(Validar(request), resultado =>
            resultado.ErrorMessage == "Debe seleccionar un servicio.");
    }

    [Fact]
    public void ServicioSembradoConIdNegativoEsValido()
    {
        var request = CrearValido();
        request.ServicioId = -106;

        Assert.Empty(Validar(request));
    }

    [Fact]
    public void ReservaCompletaEsValida()
    {
        Assert.Empty(Validar(CrearValido()));
    }

    private static CrearReservaRequest CrearValido() => new()
    {
        NombreCompleto = "Ana Pérez",
        Telefono = "+54 11 2233-4455",
        Marca = "Ford",
        Modelo = "Focus",
        ServicioId = 1,
        FechaHora = new DateTime(2026, 11, 10, 10, 0, 0)
    };

    private static List<ValidationResult> Validar(CrearReservaRequest request)
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
