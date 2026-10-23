using Lavadero.Application.Interfaces;
using Lavadero.Infrastructure.Data;
using Lavadero.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lavadero.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContextFactory<LavaderoDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(
                    typeof(LavaderoDbContext).Assembly.FullName)));

        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<CatalogoService>();
        services.AddScoped<IServicioService, ServicioService>();
        services.AddScoped<ITurnoService, TurnoService>();
        services.AddScoped<IHorarioAtencionService, HorarioAtencionService>();
        services.AddScoped<IConfiguracionLavaderoService, ConfiguracionLavaderoService>();
        services.AddScoped<IDashboardAdminService, DashboardAdminService>();

        return services;
    }

    public static async Task ApplyDatabaseMigrationsAsync(
        this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var factory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<LavaderoDbContext>>();

        await using var dbContext =
            await factory.CreateDbContextAsync();

        await dbContext.Database.MigrateAsync();
    }
}
