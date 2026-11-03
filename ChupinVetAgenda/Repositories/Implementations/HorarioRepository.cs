using ChupinVetAgenda.Data;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChupinVetAgenda.Repositories.Implementations;

public class HorarioRepository : IHorarioRepository
{
    private readonly AppDbContext _context;

    public HorarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<HorarioDisponivel>> ListarTodosAsync()
    {
        return await _context.HorariosDisponiveis.ToListAsync();
    }

    public async Task<List<HorarioDisponivel>> ListarDisponiveisAsync()
    {
        return await _context.HorariosDisponiveis
            .Where(h => h.Status == "Disponivel")
            .ToListAsync();
    }

    public async Task<HorarioDisponivel?> BuscarPorIdAsync(int id)
    {
        return await _context.HorariosDisponiveis.FindAsync(id);
    }

    public async Task<bool> PossuiAgendamentoAsync(int horarioId)
    {
        return await _context.Agendamentos
            .AnyAsync(a => a.HorarioDisponivelId == horarioId);
    }

    public async Task AdicionarAsync(HorarioDisponivel horario)
    {
        await _context.HorariosDisponiveis.AddAsync(horario);
    }

    public void Remover(HorarioDisponivel horario)
    {
        _context.HorariosDisponiveis.Remove(horario);
    }

    public async Task SalvarAlteracoesAsync()
    {
        await _context.SaveChangesAsync();
    }
}