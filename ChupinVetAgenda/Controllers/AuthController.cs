using ChupinVetAgenda.DTOs;
using ChupinVetAgenda.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChupinVetAgenda.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("cadastro")]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastroRequest request)
    {
        var cadastrado = await _authService.CadastrarAsync(request);

        if (!cadastrado)
            return Conflict(new
            {
                mensagem = "Email ja cadastrado."
            });

        return StatusCode(201, new
        {
            mensagem = "Usuario cadastrado com sucesso."
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request);

        if (token == null)
            return Unauthorized(new
            {
                mensagem = "Email ou senha invalidos."
            });

        return Ok(new
        {
            token,
            tipo = "Bearer"
        });
    }
}