using ChupinVetAgenda.Models;

namespace ChupinVetAgenda.Services.Interfaces;

public interface IAgendamentoService
{
    Task<List<Agendamento>> ListarTodosAsync();

    Task<Agendamento?> BuscarPorIdAsync(int id);

    Task<List<Agendamento>> ListarPorResponsavelAsync(string email);

    Task<List<Agendamento>> ListarVeterinarioAsync();
    
    Task<Agendamento> CriarAsync(Agendamento agendamento);

    Task<bool> AtualizarAsync(int id, Agendamento agendamento);

    Task<bool> ExcluirAsync(int id);
}