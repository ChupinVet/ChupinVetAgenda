using ChupinVetAgenda.Models;

namespace ChupinVetAgenda.Repositories.Interfaces;

public interface IHorarioRepository
{
    Task<List<HorarioDisponivel>> ListarTodosAsync();

    Task<List<HorarioDisponivel>> ListarDisponiveisAsync();

    Task<HorarioDisponivel?> BuscarPorIdAsync(int id);

    Task<bool> PossuiAgendamentoAsync(int horarioId);

    Task AdicionarAsync(HorarioDisponivel horario);

    void Remover(HorarioDisponivel horario);

    Task SalvarAlteracoesAsync();
}