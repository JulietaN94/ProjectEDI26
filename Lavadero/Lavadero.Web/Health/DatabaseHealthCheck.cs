using Lavadero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Lavadero.Web.Health;

/// <summary>
/// Verifica que la aplicación pueda conectarse a la base de datos configurada.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbContextFactory<LavaderoDbContext> _factory;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(
        IDbContextFactory<LavaderoDbContext> factory,
        ILogger<DatabaseHealthCheck> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var dbContext =
                await _factory.CreateDbContextAsync(cancellationToken);

            var disponible =
                await dbContext.Database.CanConnectAsync(cancellationToken);

            return disponible
                ? HealthCheckResult.Healthy("SQL Server disponible.")
                : HealthCheckResult.Unhealthy(
                    "No se pudo conectar con SQL Server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Falló el Health Check de la base de datos.");

            return HealthCheckResult.Unhealthy(
                "Error verificando la base de datos.",
                ex);
        }
    }
}
