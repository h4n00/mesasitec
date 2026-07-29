using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura;

public class MesaSitecDbContext : DbContext
{
    public MesaSitecDbContext(DbContextOptions<MesaSitecDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Solicitud>()
            .HasIndex(s => new { s.TenantId, s.Codigo })
            .IsUnique();
    }
}