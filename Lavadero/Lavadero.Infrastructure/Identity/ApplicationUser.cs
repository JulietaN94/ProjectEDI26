using Microsoft.AspNetCore.Identity;

namespace Lavadero.Infrastructure.Identity;

/// <summary>
/// Usuario autenticado del área administrativa.
/// Se mantiene en Infrastructure porque depende de ASP.NET Core Identity.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
