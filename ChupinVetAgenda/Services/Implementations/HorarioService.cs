using ChupinVetAgenda.Data;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChupinVetAgenda.Services.Implementations;

public class HorarioService : IHorarioService
{
    private readonly AppDbContext _context;

    public HorarioService(AppDbContext context)
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

    public async Task<HorarioDisponivel> CriarAsync(
        HorarioDisponivel horario)
    {
        _context.HorariosDisponiveis.Add(horario);

        await _context.SaveChangesAsync();

        return horario;
    }

    public async Task<bool> AtualizarAsync(
        int id,
        HorarioDisponivel horario)
    {
        var existente = await _context.HorariosDisponiveis.FindAsync(id);

        if (existente == null)
            return false;

        existente.DataHora = horario.DataHora;
        existente.TipoAtendimento = horario.TipoAtendimento;
        existente.Status = horario.Status;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        var horario = await _context.HorariosDisponiveis.FindAsync(id);

        if (horario == null)
            return false;

        var possuiAgendamento = await _context.Agendamentos
            .AnyAsync(a => a.HorarioDisponivelId == id);

        if (possuiAgendamento)
            throw new InvalidOperationException(
                "Nao e possivel remover um horario ocupado.");

        _context.HorariosDisponiveis.Remove(horario);

        await _context.SaveChangesAsync();

        return true;
    }
}