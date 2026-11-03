using ChupinVetAgenda.DTOs;

namespace ChupinVetAgenda.Services.Interfaces;

public interface IAuthService
{
    Task<bool> CadastrarAsync(CadastroRequest request);

    Task<string?> LoginAsync(LoginRequest request);
}