using ChupinVetAgenda.Data;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ChupinVetAgenda.Services;
using ChupinVetAgenda.Services.Exceptions;

namespace ChupinVetAgenda.Services.Implementations;

public class AgendamentoService : IAgendamentoService
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public AgendamentoService(
        AppDbContext context,
        EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
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
    
    
    public async Task<Agendamento> CriarAsync(Agendamento agendamento)
    {
        var horario = await _context.HorariosDisponiveis
            .FindAsync(agendamento.HorarioDisponivelId);

        if (horario == null)
            throw new HorarioNaoEncontradoException();

        if (horario.Status != "Disponivel")
            throw new HorarioIndisponivelException();

        horario.Status = "Ocupado";

        _context.Agendamentos.Add(agendamento);

        await _context.SaveChangesAsync();

        _emailService.EnviarEmailNovoAgendamento(
            agendamento.NomePet,
            agendamento.EspeciePet,
            horario.TipoAtendimento,
            horario.DataHora
        );

        return agendamento;
    }

    public async Task<bool> AtualizarAsync(
        int id,
        Agendamento agendamento)
    {
        var existente = await _context.Agendamentos
            .FindAsync(id);

        if (existente == null)
            return false;

        existente.NomeResponsavel = agendamento.NomeResponsavel;
        existente.EmailResponsavel = agendamento.EmailResponsavel;
        existente.NomePet = agendamento.NomePet;
        existente.EspeciePet = agendamento.EspeciePet;
        existente.Status = agendamento.Status;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        var agendamento = await _context.Agendamentos
            .FindAsync(id);

        if (agendamento == null)
            return false;

        var horario = await _context.HorariosDisponiveis
            .FindAsync(agendamento.HorarioDisponivelId);

        if (horario != null)
            horario.Status = "Disponivel";

        _context.Agendamentos.Remove(agendamento);

        await _context.SaveChangesAsync();

        return true;
    }
    
    
    public async Task<(List<Agendamento> Itens, int Total)> ConsultarAsync(
        int pagina,
        int tamanhoPagina,
        string? status,
        string? especie,
        string? ordenarPor,
        bool descendente)
    {
        // Consulta inicial
        var consulta = _context.Agendamentos
            .Include(a => a.HorarioDisponivel)
            .AsNoTracking()
            .AsQueryable();

        // Filtros
        if (!string.IsNullOrWhiteSpace(status))
        {
            consulta = consulta.Where(a => a.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(especie))
        {
            consulta = consulta.Where(a => a.EspeciePet == especie);
        }

        // Total de registros antes da paginacao
        var total = await consulta.CountAsync();

        // Ordenacao
        consulta = ordenarPor?.ToLowerInvariant() switch
        {
            "nomepet" => descendente
                ? consulta.OrderByDescending(a => a.NomePet)
                    .ThenBy(a => a.Id)
                : consulta.OrderBy(a => a.NomePet)
                    .ThenBy(a => a.Id),

            "nomeresponsavel" => descendente
                ? consulta.OrderByDescending(a => a.NomeResponsavel)
                    .ThenBy(a => a.Id)
                : consulta.OrderBy(a => a.NomeResponsavel)
                    .ThenBy(a => a.Id),

            "status" => descendente
                ? consulta.OrderByDescending(a => a.Status)
                    .ThenBy(a => a.Id)
                : consulta.OrderBy(a => a.Status)
                    .ThenBy(a => a.Id),

            _ => descendente
                ? consulta.OrderByDescending(a => a.Id)
                : consulta.OrderBy(a => a.Id)
        };

        // Paginacao
        var itens = await consulta
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (itens, total);
    }
}