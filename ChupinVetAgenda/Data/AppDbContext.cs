using ChupinVetAgenda.Data;using ChupinVetAgenda.Models;
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
}