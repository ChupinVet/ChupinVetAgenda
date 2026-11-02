using ChupinVetAgenda.Models;

namespace ChupinVetAgenda.Services.Interfaces;

public interface IHorarioService
{
    Task<List<HorarioDisponivel>> ListarTodosAsync();

    Task<List<HorarioDisponivel>> ListarDisponiveisAsync();

    Task<HorarioDisponivel?> BuscarPorIdAsync(int id);

    Task<HorarioDisponivel> CriarAsync(HorarioDisponivel horario);

    Task<bool> AtualizarAsync(int id, HorarioDisponivel horario);

    Task<bool> ExcluirAsync(int id);
}