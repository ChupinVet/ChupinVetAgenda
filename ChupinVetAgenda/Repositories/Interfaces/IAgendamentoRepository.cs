using ChupinVetAgenda.Models;

namespace ChupinVetAgenda.Repositories.Interfaces;

public interface IAgendamentoRepository
{
    Task<List<Agendamento>> ListarTodosAsync();

    Task<Agendamento?> BuscarPorIdAsync(int id);

    Task<List<Agendamento>> ListarPorResponsavelAsync(string email);

    Task<List<Agendamento>> ListarVeterinarioAsync();

    Task<Agendamento?> BuscarParaAtualizacaoAsync(int id);

    Task<(List<Agendamento> Itens, int Total)> ConsultarAsync(
        int pagina,
        int tamanhoPagina,
        string? status,
        string? especie,
        string? ordenarPor,
        bool descendente);

    Task AdicionarAsync(Agendamento agendamento);

    void Remover(Agendamento agendamento);

    Task SalvarAlteracoesAsync();
}