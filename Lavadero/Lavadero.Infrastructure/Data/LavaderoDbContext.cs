using Lavadero.Domain.Entities;
using Lavadero.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace Lavadero.Infrastructure.Data;

public sealed class LavaderoDbContext : DbContext
{
    public LavaderoDbContext(DbContextOptions<LavaderoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Auto> Autos => Set<Auto>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<HorarioAtencion> HorariosAtencion => Set<HorarioAtencion>();
    public DbSet<UsuarioAdministrador> UsuariosAdministradores => Set<UsuarioAdministrador>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LavaderoDbContext).Assembly);
        DatabaseSeeder.Seed(modelBuilder);
    }
}
