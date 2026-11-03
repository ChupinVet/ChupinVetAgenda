using ChupinVetAgenda.Models;

namespace ChupinVetAgenda.Repositories.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorEmailAsync(string email);

    Task AdicionarAsync(Usuario usuario);

    Task SalvarAlteracoesAsync();
}