using System.ComponentModel.DataAnnotations;

namespace Lavadero.Application.Common;

public static class ValidacionRequest
{
    public static void Validar(object request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var errores = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new ValidationContext(request), errores, true))
            throw new InvalidOperationException(errores[0].ErrorMessage);
    }
}
