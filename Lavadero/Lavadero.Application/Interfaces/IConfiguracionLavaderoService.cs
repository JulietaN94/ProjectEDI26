using Lavadero.Application.DTOs;
using Lavadero.Application.Models;

namespace Lavadero.Application.Interfaces;

public interface IConfiguracionLavaderoService
{
    Task<ConfiguracionLavaderoDto> ObtenerAsync();
    Task<LogoLavaderoDto?> ObtenerLogoAsync();

    Task<ConfiguracionLavaderoDto> ActualizarAsync(
        GuardarConfiguracionLavaderoRequest request);

    Task GuardarLogoAsync(
        byte[] contenido,
        string contentType);

    Task EliminarLogoAsync();
}
