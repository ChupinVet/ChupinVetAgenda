using ChupinVetAgenda.Data;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChupinVetAgenda.Repositories.Implementations;

public class AgendamentoRepository : IAgendamentoRepository
{
    private readonly AppDbContext _context;

    public AgendamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Agendamento>> ListarTodosAsync()
    {
        return await _context.Agendamentos
            .Include(a => a.HorarioDisponivel)
            .ToListAsync();
    }

    public async Task<Agendamento?> BuscarPorIdAsync(int id)
    {
        return await _context.Agendamentos
            .Include(a => a.HorarioDisponivel)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Agendamento>> ListarPorResponsavelAsync(string email)
    {
        return await _context.Agendamentos
            .Include(a => a.HorarioDisponivel)
            .Where(a => a.EmailResponsavel == email)
            .ToListAsync();
    }

    public async Task<List<Agendamento>> ListarVeterinarioAsync()
    {
        return await _context.Agendamentos
            .Include(a => a.HorarioDisponivel)
            .Where(a => a.Status == "Marcado")
            .ToListAsync();
    }

    public async Task<Agendamento?> BuscarParaAtualizacaoAsync(int id)
    {
        return await _context.Agendamentos.FindAsync(id);
    }

    public async Task<(List<Agendamento> Itens, int Total)> ConsultarAsync(
        int pagina,
        int tamanhoPagina,
        string? status,
        string? especie,
        string? ordenarPor,
        bool descendente)
    {
        var consulta = _context.Agendamentos
            .Include(a => a.HorarioDisponivel)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            consulta = consulta.Where(a => a.Status == status);

        if (!string.IsNullOrWhiteSpace(especie))
            consulta = consulta.Where(a => a.EspeciePet == especie);

        var total = await consulta.CountAsync();

        consulta = ordenarPor?.ToLowerInvariant() switch
        {
            "nomepet" => descendente
                ? consulta.OrderByDescending(a => a.NomePet).ThenBy(a => a.Id)
                : consulta.OrderBy(a => a.NomePet).ThenBy(a => a.Id),

            "nomeresponsavel" => descendente
                ? consulta.OrderByDescending(a => a.NomeResponsavel).ThenBy(a => a.Id)
                : consulta.OrderBy(a => a.NomeResponsavel).ThenBy(a => a.Id),

            "status" => descendente
                ? consulta.OrderByDescending(a => a.Status).ThenBy(a => a.Id)
                : consulta.OrderBy(a => a.Status).ThenBy(a => a.Id),

            _ => descendente
                ? consulta.OrderByDescending(a => a.Id)
                : consulta.OrderBy(a => a.Id)
        };

        var itens = await consulta
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (itens, total);
    }

    public async Task AdicionarAsync(Agendamento agendamento)
    {
        await _context.Agendamentos.AddAsync(agendamento);
    }

    public void Remover(Agendamento agendamento)
    {
        _context.Agendamentos.Remove(agendamento);
    }

    public async Task SalvarAlteracoesAsync()
    {
        await _context.SaveChangesAsync();
    }
}