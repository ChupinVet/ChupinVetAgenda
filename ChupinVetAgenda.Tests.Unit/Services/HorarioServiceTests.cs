using ChupinVetAgenda.Data;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using ChupinVetAgenda.Repositories.Implementations;

namespace ChupinVetAgenda.Tests.Unit.Services;

public class HorarioServiceTests
{
    private static AppDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task ExcluirAsync_HorarioComAgendamento_LancaExcecao()
    {
        // Arrange
        using var context = CriarContexto();

        var horario = new HorarioDisponivel
        {
            Id = 1,
            DataHora = new DateTime(2026, 9, 20, 10, 0, 0),
            TipoAtendimento = "Consulta",
            Status = "Ocupado"
        };

        var agendamento = new Agendamento
        {
            Id = 1,
            HorarioDisponivelId = 1,
            NomeResponsavel = "Teste",
            EmailResponsavel = "teste@email.com",
            NomePet = "Thor",
            EspeciePet = "Cachorro",
            Status = "Marcado"
        };

        context.HorariosDisponiveis.Add(horario);
        context.Agendamentos.Add(agendamento);

        await context.SaveChangesAsync();

        var horarioRepository = new HorarioRepository(context);

        var service = new HorarioService(horarioRepository);

        // Act e Assert
        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExcluirAsync(1)
        );

        Assert.Equal(
            "Nao e possivel remover um horario ocupado.",
            excecao.Message
        );

        // verifica se o horario continua no banco
        Assert.NotNull(
            await context.HorariosDisponiveis.FindAsync(1)
        );
    }
    
    [Fact]
    public async Task ExcluirAsync_HorarioInexistente_RetornaFalse()
    {
        // Arrange
        using var context = CriarContexto();

        var horarioRepository = new HorarioRepository(context);
        var service = new HorarioService(horarioRepository);

        // Act
        var resultado = await service.ExcluirAsync(999);

        // Assert
        Assert.False(resultado);
        Assert.Empty(context.HorariosDisponiveis);
    }
    
    [Fact]
    public async Task ExcluirAsync_HorarioSemAgendamento_ExcluiERetornaTrue()
    {
        // Arrange
        using var context = CriarContexto();

        var horario = new HorarioDisponivel
        {
            Id = 1,
            DataHora = new DateTime(2026, 11, 20, 10, 0, 0),
            TipoAtendimento = "Consulta",
            Status = "Disponivel"
        };

        context.HorariosDisponiveis.Add(horario);
        await context.SaveChangesAsync();

        var horarioRepository = new HorarioRepository(context);
        var service = new HorarioService(horarioRepository);

        // Act
        var resultado = await service.ExcluirAsync(1);

        // Assert
        Assert.True(resultado);

        context.ChangeTracker.Clear();

        var horarioExcluido =
            await context.HorariosDisponiveis.FindAsync(1);

        Assert.Null(horarioExcluido);
    }
    
    [Fact]
    public async Task AtualizarAsync_HorarioInexistente_RetornaFalse()
    {
        // Arrange
        using var context = CriarContexto();

        var horarioRepository = new HorarioRepository(context);
        var service = new HorarioService(horarioRepository);

        var horarioAtualizado = new HorarioDisponivel
        {
            DataHora = new DateTime(2026, 11, 25, 14, 0, 0),
            TipoAtendimento = "Retorno",
            Status = "Disponivel"
        };

        // Act
        var resultado = await service.AtualizarAsync(
            999, horarioAtualizado
        );

        // Assert
        Assert.False(resultado);
        Assert.Empty(context.HorariosDisponiveis);
    }
    
    [Fact]
    public async Task AtualizarAsync_HorarioExistente_AtualizaDadosERetornaTrue()
    {
        // Arrange
        using var context = CriarContexto();

        var horario = new HorarioDisponivel
        {
            Id = 1,
            DataHora = new DateTime(2026, 11, 20, 10, 0, 0),
            TipoAtendimento = "Consulta",
            Status = "Disponivel"
        };

        context.HorariosDisponiveis.Add(horario);
        await context.SaveChangesAsync();

        var horarioRepository = new HorarioRepository(context);
        var service = new HorarioService(horarioRepository);

        var horarioAtualizado = new HorarioDisponivel
        {
            DataHora = new DateTime(2026, 11, 25, 14, 0, 0),
            TipoAtendimento = "Retorno",
            Status = "Ocupado"
        };

        // Act
        var resultado = await service.AtualizarAsync(
            1, horarioAtualizado
        );

        // Assert
        Assert.True(resultado);

        context.ChangeTracker.Clear();

        var horarioSalvo =
            await context.HorariosDisponiveis.FindAsync(1);

        Assert.NotNull(horarioSalvo);
        Assert.Equal(horarioAtualizado.DataHora, horarioSalvo.DataHora);
        Assert.Equal(horarioAtualizado.TipoAtendimento, horarioSalvo.TipoAtendimento);
        Assert.Equal(horarioAtualizado.Status, horarioSalvo.Status);
    }
}