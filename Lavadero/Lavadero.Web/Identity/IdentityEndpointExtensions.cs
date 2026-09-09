using Lavadero.Infrastructure.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;

namespace Lavadero.Web.Identity;

public static class IdentityEndpointExtensions
{
    public static IEndpointRouteBuilder MapLavaderoIdentityEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/account/login",
            async (
                HttpContext httpContext,
                IAntiforgery antiforgery,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager) =>
            {
                await antiforgery.ValidateRequestAsync(httpContext);

                var form = await httpContext.Request.ReadFormAsync();

                var usuario = form["usuario"].ToString().Trim();
                var password = form["password"].ToString();
                var recordarme = string.Equals(
                    form["recordarme"].ToString(),
                    "true",
                    StringComparison.OrdinalIgnoreCase);

                var returnUrl = ObtenerRetornoSeguro(
                    form["returnUrl"].ToString(),
                    "/admin");

                if (string.IsNullOrWhiteSpace(usuario) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    return Results.LocalRedirect(
                        ConstruirLoginConError(
                            "credenciales",
                            returnUrl));
                }

                var user = await userManager.FindByNameAsync(usuario);

                if (user is null || !user.Activo)
                {
                    return Results.LocalRedirect(
                        ConstruirLoginConError(
                            "credenciales",
                            returnUrl));
                }

                var resultado = await signInManager.PasswordSignInAsync(
                    user,
                    password,
                    recordarme,
                    lockoutOnFailure: true);

                if (resultado.Succeeded)
                    return Results.LocalRedirect(returnUrl);

                if (resultado.IsLockedOut)
                {
                    return Results.LocalRedirect(
                        ConstruirLoginConError(
                            "bloqueado",
                            returnUrl));
                }

                return Results.LocalRedirect(
                    ConstruirLoginConError(
                        "credenciales",
                        returnUrl));
            });

        endpoints.MapPost(
            "/account/logout",
            async (
                HttpContext httpContext,
                IAntiforgery antiforgery,
                SignInManager<ApplicationUser> signInManager) =>
            {
                await antiforgery.ValidateRequestAsync(httpContext);
                await signInManager.SignOutAsync();

                return Results.LocalRedirect("/");
            });

        return endpoints;
    }

    private static string ConstruirLoginConError(
        string error,
        string returnUrl) =>
        $"/login?error={Uri.EscapeDataString(error)}" +
        $"&returnUrl={Uri.EscapeDataString(returnUrl)}";

    private static string ObtenerRetornoSeguro(
        string? returnUrl,
        string valorPredeterminado)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return valorPredeterminado;

        if (!returnUrl.StartsWith("/", StringComparison.Ordinal) ||
            returnUrl.StartsWith("//", StringComparison.Ordinal) ||
            returnUrl.StartsWith(@"/\", StringComparison.Ordinal))
        {
            return valorPredeterminado;
        }

        return returnUrl;
    }
}
