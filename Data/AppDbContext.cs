using Microsoft.EntityFrameworkCore;
using UsuariosApi.Entities;

namespace UsuariosApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).IsRequired().HasMaxLength(120);
            e.Property(x => x.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.SenhaHash).IsRequired();
            e.Property(x => x.Perfil).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.Ativo).IsRequired();
        });
    }
}
