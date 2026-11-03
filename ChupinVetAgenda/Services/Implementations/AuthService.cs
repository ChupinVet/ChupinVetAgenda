using ChupinVetAgenda.DTOs;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Repositories.Interfaces;
using ChupinVetAgenda.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ChupinVetAgenda.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher<Usuario> passwordHasher,
        IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }
    
    public async Task<bool> CadastrarAsync(CadastroRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var usuarioExistente =
            await _usuarioRepository.BuscarPorEmailAsync(email);

        if (usuarioExistente != null)
            return false;

        var usuario = new Usuario
        {
            Nome = request.Nome.Trim(),
            Email = email
        };

        usuario.SenhaHash = _passwordHasher.HashPassword(
            usuario,
            request.Senha
        );

        await _usuarioRepository.AdicionarAsync(usuario);
        await _usuarioRepository.SalvarAlteracoesAsync();

        return true;
    }
    
    public async Task<string?> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var usuario = await _usuarioRepository.BuscarPorEmailAsync(email);

        if (usuario == null)
            return null;

        var resultado = _passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.SenhaHash,
            request.Senha
        );

        if (resultado == PasswordVerificationResult.Failed)
            return null;

        var jwtKey = _configuration["Jwt:Key"]
                     ?? throw new InvalidOperationException(
                         "A chave JWT nao foi configurada.");

        var chave = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        );

        var credenciais = new SigningCredentials(
            chave,
            SecurityAlgorithms.HmacSha256
        );

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credenciais
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}