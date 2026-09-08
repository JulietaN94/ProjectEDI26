using Lavadero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Lavadero.Web.Data;

/// <summary>
/// Crea el DbContext durante las operaciones de diseño de Entity Framework Core
/// (Add-Migration, Update-Database, etc.).
/// </summary>
public sealed class LavaderoDbContextFactory
    : IDesignTimeDbContextFactory<LavaderoDbContext>
{
    public LavaderoDbContext CreateDbContext(string[] args)
    {
        string rutaWeb = BuscarRutaProyectoWeb();

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(rutaWeb)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddUserSecrets<LavaderoDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'DefaultConnection'.");

        DbContextOptions<LavaderoDbContext> options =
            new DbContextOptionsBuilder<LavaderoDbContext>()
                .UseSqlServer(
                    connectionString,
                    sql => sql.MigrationsAssembly(
                        typeof(LavaderoDbContext).Assembly.FullName))
                .Options;

        return new LavaderoDbContext(options);
    }

    private static string BuscarRutaProyectoWeb()
    {
        string? encontrada = BuscarDesde(Directory.GetCurrentDirectory());
        if (encontrada is not null)
            return encontrada;

        encontrada = BuscarDesde(AppContext.BaseDirectory);
        if (encontrada is not null)
            return encontrada;

        throw new DirectoryNotFoundException(
            "No se pudo localizar la carpeta del proyecto Lavadero.Web.");
    }

    private static string? BuscarDesde(string rutaInicial)
    {
        DirectoryInfo? directorio = new DirectoryInfo(rutaInicial);

        while (directorio is not null)
        {
            string proyectoActual = Path.Combine(
                directorio.FullName,
                "Lavadero.Web.csproj");

            if (File.Exists(proyectoActual))
                return directorio.FullName;

            string carpetaWeb = Path.Combine(
                directorio.FullName,
                "Lavadero.Web");

            string proyectoWeb = Path.Combine(
                carpetaWeb,
                "Lavadero.Web.csproj");

            if (File.Exists(proyectoWeb))
                return carpetaWeb;

            directorio = directorio.Parent;
        }

        return null;
    }
}
