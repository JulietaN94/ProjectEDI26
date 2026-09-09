using Lavadero.Domain.Entities;
using Lavadero.Infrastructure.Data.Seed;
using Lavadero.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lavadero.Infrastructure.Data;

public sealed class LavaderoDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole, string>
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
    public DbSet<ConfiguracionLavadero> ConfiguracionLavadero => Set<ConfiguracionLavadero>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity configura aquí AspNetUsers, AspNetRoles, AspNetUserRoles, etc.
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(LavaderoDbContext).Assembly);

        DatabaseSeeder.Seed(modelBuilder);
    }
}
