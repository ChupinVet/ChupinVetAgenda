using ChupinVetAgenda.DTOs;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Repositories.Interfaces;
using ChupinVetAgenda.Services.Implementations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ChupinVetAgenda.Tests.Unit.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task CadastrarAsync_UsuarioNovo_RetornaTrue()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        var passwordHasher = new PasswordHasher<Usuario>();
        var configurationMock = new Mock<IConfiguration>();

        var request = new CadastroRequest
        {
            Nome = "Usuario Teste",
            Email = "teste@email.com",
            Senha = "Teste12345!"
        };

        repositoryMock
            .Setup(r => r.BuscarPorEmailAsync(request.Email))
            .ReturnsAsync((Usuario?)null);

        var service = new AuthService(
            repositoryMock.Object,
            passwordHasher,
            configurationMock.Object
        );

        // Act
        var resultado = await service.CadastrarAsync(request);

        // Assert
        Assert.True(resultado);

        repositoryMock.Verify(
            r => r.AdicionarAsync(
                It.Is<Usuario>(u =>
                    u.Email == request.Email &&
                    !string.IsNullOrEmpty(u.SenhaHash) &&
                    u.SenhaHash != request.Senha
                )
            ),
            Times.Once
        );

        repositoryMock.Verify(
            r => r.SalvarAlteracoesAsync(),
            Times.Once
        );
    }
    
    [Fact]
    public async Task CadastrarAsync_EmailDuplicado_RetornaFalse()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        var passwordHasher = new PasswordHasher<Usuario>();
        var configurationMock = new Mock<IConfiguration>();

        var request = new CadastroRequest
        {
            Nome = "Usuario Teste",
            Email = "teste@email.com",
            Senha = "Teste12345!"
        };

        var usuarioExistente = new Usuario
        {
            Id = 1,
            Nome = "Usuario Existente",
            Email = "teste@email.com",
            SenhaHash = "hash-existente"
        };

        repositoryMock
            .Setup(r => r.BuscarPorEmailAsync(request.Email))
            .ReturnsAsync(usuarioExistente);

        var service = new AuthService(
            repositoryMock.Object,
            passwordHasher,
            configurationMock.Object
        );

        // Act
        var resultado = await service.CadastrarAsync(request);

        // Assert
        Assert.False(resultado);

        repositoryMock.Verify(
            r => r.AdicionarAsync(It.IsAny<Usuario>()),
            Times.Never
        );

        repositoryMock.Verify(
            r => r.SalvarAlteracoesAsync(),
            Times.Never
        );
    }
    
    [Fact]
    public async Task LoginAsync_CredenciaisCorretas_RetornaToken()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        var passwordHasher = new PasswordHasher<Usuario>();

        var usuario = new Usuario
        {
            Id = 1,
            Nome = "Usuario Teste",
            Email = "teste@email.com"
        };

        usuario.SenhaHash = passwordHasher.HashPassword(
            usuario,
            "Teste12345!"
        );

        repositoryMock
            .Setup(r => r.BuscarPorEmailAsync(usuario.Email))
            .ReturnsAsync(usuario);
        
        var configuracoes = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "ChaveDeTesteJWTComMaisDe32Caracteres123456789",
            ["Jwt:Issuer"] = "ChupinVetAgenda",
            ["Jwt:Audience"] = "ChupinVetAgendaAPI"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configuracoes)
            .Build();

        var service = new AuthService(
            repositoryMock.Object,
            passwordHasher,
            configuration
        );

        var request = new LoginRequest
        {
            Email = "teste@email.com",
            Senha = "Teste12345!"
        };

        // Act
        var token = await service.LoginAsync(request);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));

        var partes = token!.Split('.');
        Assert.Equal(3, partes.Length);
        
        var chave = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuracoes["Jwt:Key"]!)
        );

        var parametros = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = chave,
            ValidateIssuer = true,
            ValidIssuer = configuracoes["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuracoes["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();

        var principal = handler.ValidateToken(
            token,
            parametros,
            out var tokenValidado
        );

        Assert.NotNull(principal);

        var jwt = Assert.IsType<JwtSecurityToken>(tokenValidado);

        Assert.True(jwt.ValidTo > DateTime.UtcNow);
        Assert.True(jwt.ValidTo <= DateTime.UtcNow.AddHours(1));

        repositoryMock.Verify(
            r => r.BuscarPorEmailAsync(usuario.Email),
            Times.Once
        );
    }
    
    [Fact]
    public async Task LoginAsync_SenhaIncorreta_RetornaNull()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        var passwordHasher = new PasswordHasher<Usuario>();
        var configurationMock = new Mock<IConfiguration>();

        var usuario = new Usuario
        {
            Id = 1,
            Nome = "Usuario Teste",
            Email = "teste@email.com"
        };

        usuario.SenhaHash = passwordHasher.HashPassword(
            usuario,
            "SenhaCorreta123!"
        );

        repositoryMock
            .Setup(r => r.BuscarPorEmailAsync(usuario.Email))
            .ReturnsAsync(usuario);

        var service = new AuthService(
            repositoryMock.Object,
            passwordHasher,
            configurationMock.Object
        );

        var request = new LoginRequest
        {
            Email = "teste@email.com",
            Senha = "SenhaErrada123!"
        };

        // Act
        var resultado = await service.LoginAsync(request);

        // Assert
        Assert.Null(resultado);

        repositoryMock.Verify(
            r => r.BuscarPorEmailAsync(usuario.Email),
            Times.Once
        );

        repositoryMock.Verify(
            r => r.AdicionarAsync(It.IsAny<Usuario>()),
            Times.Never
        );
    }
    
    [Fact]
    public async Task LoginAsync_UsuarioInexistente_RetornaNull()
    {
        // Arrange
        var repositoryMock = new Mock<IUsuarioRepository>();
        var passwordHasher = new PasswordHasher<Usuario>();
        var configurationMock = new Mock<IConfiguration>();

        repositoryMock
            .Setup(r => r.BuscarPorEmailAsync("inexistente@email.com"))
            .ReturnsAsync((Usuario?)null);

        var service = new AuthService(
            repositoryMock.Object,
            passwordHasher,
            configurationMock.Object
        );

        var request = new LoginRequest
        {
            Email = "inexistente@email.com",
            Senha = "Teste12345!"
        };

        // Act
        var resultado = await service.LoginAsync(request);

        // Assert
        Assert.Null(resultado);

        repositoryMock.Verify(
            r => r.BuscarPorEmailAsync(request.Email),
            Times.Once
        );

        repositoryMock.Verify(
            r => r.AdicionarAsync(It.IsAny<Usuario>()),
            Times.Never
        );
    }
}