using ChupinVetAgenda.Models;
using Microsoft.EntityFrameworkCore;

namespace ChupinVetAgenda.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<HorarioDisponivel> HorariosDisponiveis { get; set; }

    public DbSet<Agendamento> Agendamentos { get; set; }
    
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}