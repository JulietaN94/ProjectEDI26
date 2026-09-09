using Lavadero.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lavadero.Infrastructure.Extensions;

public static class IdentitySeedExtensions
{
    private const string RolAdministrador = "Administrador";

    public static async Task SeedAdminIdentityAsync(
        this IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILogger<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync(RolAdministrador))
        {
            var resultadoRol = await roleManager.CreateAsync(
                new IdentityRole(RolAdministrador));

            if (!resultadoRol.Succeeded)
            {
                throw new InvalidOperationException(
                    "No se pudo crear el rol Administrador: " +
                    string.Join(
                        "; ",
                        resultadoRol.Errors.Select(x => x.Description)));
            }
        }

        var usuario = configuration["AdminSeed:Usuario"];
        var email = configuration["AdminSeed:Email"];
        var nombreCompleto = configuration["AdminSeed:NombreCompleto"];
        var password = configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(usuario) ||
            string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "No se creó el usuario administrador inicial. " +
                "Configurá AdminSeed:Usuario y AdminSeed:Password mediante User Secrets.");
            return;
        }

        var existente = await userManager.FindByNameAsync(usuario);

        if (existente is null)
        {
            existente = new ApplicationUser
            {
                UserName = usuario.Trim(),
                Email = string.IsNullOrWhiteSpace(email)
                    ? null
                    : email.Trim(),
                EmailConfirmed = true,
                NombreCompleto = string.IsNullOrWhiteSpace(nombreCompleto)
                    ? "Administrador"
                    : nombreCompleto.Trim(),
                Activo = true
            };

            var resultadoUsuario = await userManager.CreateAsync(
                existente,
                password);

            if (!resultadoUsuario.Succeeded)
            {
                throw new InvalidOperationException(
                    "No se pudo crear el usuario administrador inicial: " +
                    string.Join(
                        "; ",
                        resultadoUsuario.Errors.Select(x => x.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(
                existente,
                RolAdministrador))
        {
            var resultadoRolUsuario =
                await userManager.AddToRoleAsync(
                    existente,
                    RolAdministrador);

            if (!resultadoRolUsuario.Succeeded)
            {
                throw new InvalidOperationException(
                    "No se pudo asignar el rol Administrador al usuario: " +
                    string.Join(
                        "; ",
                        resultadoRolUsuario.Errors.Select(x => x.Description)));
            }
        }
    }
}
