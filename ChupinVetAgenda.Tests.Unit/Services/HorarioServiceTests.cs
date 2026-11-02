using ChupinVetAgenda.Data;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Services.Implementations;
using Microsoft.EntityFrameworkCore;

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

        var service = new HorarioService(context);

        // Act e Assert
        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExcluirAsync(1)
        );

        Assert.Equal(
            "Nao e possivel remover um horario ocupado.",
            excecao.Message
        );

        // Verifica se o horario continua no banco
        Assert.NotNull(
            await context.HorariosDisponiveis.FindAsync(1)
        );
    }
}