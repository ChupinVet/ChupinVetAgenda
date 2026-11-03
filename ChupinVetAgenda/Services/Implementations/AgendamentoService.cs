using ChupinVetAgenda.Data;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ChupinVetAgenda.Services;
using ChupinVetAgenda.Services.Exceptions;
using ChupinVetAgenda.Repositories.Interfaces;

namespace ChupinVetAgenda.Services.Implementations;

public class AgendamentoService : IAgendamentoService
{
    
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IHorarioRepository _horarioRepository;
    private readonly EmailService _emailService;

    public AgendamentoService(
        IAgendamentoRepository agendamentoRepository,
        IHorarioRepository horarioRepository,
        EmailService emailService)
    {
        _agendamentoRepository = agendamentoRepository;
        _horarioRepository = horarioRepository;
        _emailService = emailService;
    }
    
    public async Task<List<Agendamento>> ListarTodosAsync()
    {
        return await _agendamentoRepository.ListarTodosAsync();
    }

    public async Task<Agendamento?> BuscarPorIdAsync(int id)
    {
        return await _agendamentoRepository.BuscarPorIdAsync(id);
    }

    public async Task<List<Agendamento>> ListarPorResponsavelAsync(string email)
    {
        return await _agendamentoRepository.ListarPorResponsavelAsync(email);
    }

    public async Task<List<Agendamento>> ListarVeterinarioAsync()
    {
        return await _agendamentoRepository.ListarVeterinarioAsync();
    }
    
    
    public async Task<Agendamento> CriarAsync(Agendamento agendamento)
    {
        var horario = await _horarioRepository
            .BuscarPorIdAsync(agendamento.HorarioDisponivelId);

        if (horario == null)
            throw new HorarioNaoEncontradoException();

        if (horario.Status != "Disponivel")
            throw new HorarioIndisponivelException();

        horario.Status = "Ocupado";

        await _agendamentoRepository.AdicionarAsync(agendamento);

        await _agendamentoRepository.SalvarAlteracoesAsync();

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
        var existente = await _agendamentoRepository
            .BuscarParaAtualizacaoAsync(id);

        if (existente == null)
            return false;

        existente.NomeResponsavel = agendamento.NomeResponsavel;
        existente.EmailResponsavel = agendamento.EmailResponsavel;
        existente.NomePet = agendamento.NomePet;
        existente.EspeciePet = agendamento.EspeciePet;
        existente.Status = agendamento.Status;

        await _agendamentoRepository.SalvarAlteracoesAsync();

        return true;
    }
    
    public async Task<bool> ExcluirAsync(int id)
    {
        var agendamento = await _agendamentoRepository
            .BuscarParaAtualizacaoAsync(id);

        if (agendamento == null)
            return false;

        var horario = await _horarioRepository
            .BuscarPorIdAsync(agendamento.HorarioDisponivelId);

        if (horario != null)
            horario.Status = "Disponivel";

        _agendamentoRepository.Remover(agendamento);

        await _agendamentoRepository.SalvarAlteracoesAsync();

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
        return await _agendamentoRepository.ConsultarAsync(
            pagina,
            tamanhoPagina,
            status,
            especie,
            ordenarPor,
            descendente
        );
    }
}