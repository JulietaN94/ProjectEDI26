using Lavadero.Application.Interfaces;
using Lavadero.Infrastructure.Data;
using Lavadero.Infrastructure.Extensions;
using Lavadero.Infrastructure.Identity;
using Lavadero.Web.Components;
using Lavadero.Web.Health;
using Lavadero.Web.Identity;
using Lavadero.Web.State;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1_000_000;
});

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontró una cadena de conexión válida en " +
        "'ConnectionStrings:DefaultConnection'.");
}

builder.Services.AddInfrastructure(connectionString);

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = false;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(5);

        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<LavaderoDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/acceso-denegado";
    options.Cookie.Name = "Lavadero.Admin";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);

    // Como los endpoints de logo usan un POST multipart tradicional,
    // reforzamos la cookie de administración.
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddScoped<ReservaState>();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapLavaderoIdentityEndpoints();

app.MapHealthChecks("/health");
app.MapGet(
    "/branding/favicon",
    async (
        IConfiguracionLavaderoService servicio,
        IWebHostEnvironment env) =>
    {
        var logo = await servicio.ObtenerLogoAsync();

        if (logo is not null)
        {
            return Results.File(
                logo.Contenido,
                logo.ContentType);
        }

        var rutaFallback =
            Path.Combine(env.WebRootPath, "favicon.png");

        if (File.Exists(rutaFallback))
        {
            return Results.File(
                rutaFallback,
                "image/png");
        }

        return Results.NotFound();
    })
    .AllowAnonymous();

app.MapGet(
    "/branding/logo",
    async (IConfiguracionLavaderoService servicio) =>
    {
        var logo =
            await servicio.ObtenerLogoAsync();

        return logo is null
            ? Results.NotFound()
            : Results.File(
                logo.Contenido,
                logo.ContentType);
    })
    .AllowAnonymous();

app.MapGet(
    "/productos/{id:int}/imagen",
    async (int id, ICatalogoService catalogo, HttpContext context) =>
    {
        var imagen = await catalogo.ObtenerImagenProductoAsync(id);
        if (imagen is null)
            return Results.NotFound();

        context.Response.Headers.CacheControl = "no-store";
        return Results.File(imagen.Contenido, imagen.ContentType);
    })
    .AllowAnonymous();

app.MapPost(
    "/branding/logo/upload",
    async (
        HttpContext httpContext,
        IFormFile logo,
        IConfiguracionLavaderoService servicio,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken) =>
    {
        var logger =
            loggerFactory.CreateLogger(
                "LogoUpload");

        try
        {
            // La validación antiforgery automática de IFormFile se desactiva
            // SOLO en este endpoint. En su lugar exigimos:
            // 1) usuario Administrador (metadata del endpoint),
            // 2) cookie SameSite=Strict,
            // 3) POST originado desde el mismo sitio.
            if (!EsMismoOrigen(httpContext))
            {
                logger.LogWarning(
                    "Intento de carga de logo rechazado por origen distinto.");

                return Results.StatusCode(
                    StatusCodes.Status403Forbidden);
            }

            logger.LogInformation(
                "Inicio de carga de logo. Nombre={Nombre}, Tipo={Tipo}, Bytes={Bytes}",
                logo.FileName,
                logo.ContentType,
                logo.Length);

            if (logo.Length <= 0 ||
                logo.Length > 500_000)
            {
                return Results.Redirect(
                    "/admin/configuracion?logo=invalid");
            }

            var tiposPermitidos =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    "image/png",
                    "image/jpeg",
                    "image/webp"
                };

            if (!tiposPermitidos.Contains(
                    logo.ContentType))
            {
                return Results.Redirect(
                    "/admin/configuracion?logo=invalid");
            }

            await using var stream =
                logo.OpenReadStream();

            using var memoria =
                new MemoryStream();

            await stream.CopyToAsync(
                memoria,
                cancellationToken);

            logger.LogInformation(
                "Archivo leído correctamente. Bytes={Bytes}",
                memoria.Length);

            await servicio.GuardarLogoAsync(
                memoria.ToArray(),
                logo.ContentType);

            logger.LogInformation(
                "Logo guardado correctamente.");

            return Results.Redirect(
                "/admin/configuracion?logo=ok");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning(
                "La carga del logo fue cancelada.");

            return Results.Redirect(
                "/admin/configuracion?logo=error");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error inesperado cargando el logo.");

            return Results.Redirect(
                "/admin/configuracion?logo=error");
        }
    })
    .RequireAuthorization(
        policy => policy.RequireRole("Administrador"))
    .DisableAntiforgery();

app.MapPost(
    "/branding/logo/delete",
    async (
        HttpContext httpContext,
        IConfiguracionLavaderoService servicio,
        ILoggerFactory loggerFactory) =>
    {
        var logger =
            loggerFactory.CreateLogger(
                "LogoUpload");

        try
        {
            if (!EsMismoOrigen(httpContext))
            {
                logger.LogWarning(
                    "Intento de eliminación de logo rechazado por origen distinto.");

                return Results.StatusCode(
                    StatusCodes.Status403Forbidden);
            }

            await servicio.EliminarLogoAsync();

            return Results.Redirect(
                "/admin/configuracion?logo=deleted");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error eliminando el logo.");

            return Results.Redirect(
                "/admin/configuracion?logo=error");
        }
    })
    .RequireAuthorization(
        policy => policy.RequireRole("Administrador"))
    .DisableAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.Services.ApplyDatabaseMigrationsAsync();

await app.Services.SeedAdminIdentityAsync(
    builder.Configuration);

app.Run();

static bool EsMismoOrigen(HttpContext context)
{
    var request = context.Request;

    var esperado =
        $"{request.Scheme}://{request.Host}";

    var origin =
        request.Headers.Origin.ToString();

    if (!string.IsNullOrWhiteSpace(origin))
    {
        return string.Equals(
            origin.TrimEnd('/'),
            esperado.TrimEnd('/'),
            StringComparison.OrdinalIgnoreCase);
    }

    var referer =
        request.Headers.Referer.ToString();

    if (Uri.TryCreate(
            referer,
            UriKind.Absolute,
            out var uri))
    {
        var origenReferer =
            $"{uri.Scheme}://{uri.Authority}";

        return string.Equals(
            origenReferer.TrimEnd('/'),
            esperado.TrimEnd('/'),
            StringComparison.OrdinalIgnoreCase);
    }

    return false;
}
