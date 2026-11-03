using ChupinVetAgenda.Data;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ChupinVetAgenda.Repositories.Interfaces;

namespace ChupinVetAgenda.Services.Implementations;

public class HorarioService : IHorarioService
{
    private readonly IHorarioRepository _horarioRepository;

    public HorarioService(IHorarioRepository horarioRepository)
    {
        _horarioRepository = horarioRepository;
    }
    
    public async Task<List<HorarioDisponivel>> ListarTodosAsync()
    {
        return await _horarioRepository.ListarTodosAsync();
    }

    public async Task<List<HorarioDisponivel>> ListarDisponiveisAsync()
    {
        return await _horarioRepository.ListarDisponiveisAsync();
    }

    public async Task<HorarioDisponivel?> BuscarPorIdAsync(int id)
    {
        return await _horarioRepository.BuscarPorIdAsync(id);
    }
    
    public async Task<HorarioDisponivel> CriarAsync(
        HorarioDisponivel horario)
    {
        await _horarioRepository.AdicionarAsync(horario);

        await _horarioRepository.SalvarAlteracoesAsync();

        return horario;
    }

    public async Task<bool> AtualizarAsync(
        int id,
        HorarioDisponivel horario)
    {
        var existente = await _horarioRepository.BuscarPorIdAsync(id);

        if (existente == null)
            return false;

        existente.DataHora = horario.DataHora;
        existente.TipoAtendimento = horario.TipoAtendimento;
        existente.Status = horario.Status;

        await _horarioRepository.SalvarAlteracoesAsync();

        return true;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        var horario = await _horarioRepository.BuscarPorIdAsync(id);

        if (horario == null)
            return false;

        var possuiAgendamento =
            await _horarioRepository.PossuiAgendamentoAsync(id);

        if (possuiAgendamento)
            throw new InvalidOperationException(
                "Nao e possivel remover um horario ocupado.");

        _horarioRepository.Remover(horario);

        await _horarioRepository.SalvarAlteracoesAsync();

        return true;
    }
}