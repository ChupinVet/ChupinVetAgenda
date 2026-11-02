using ChupinVetAgenda.Data;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Services;
using ChupinVetAgenda.Services.Exceptions;
using ChupinVetAgenda.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace ChupinVetAgenda.Tests.Unit.Services;

public class AgendamentoServiceTests
{
    private static AppDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CriarAsync_HorarioOcupado_LancaExcecao()
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

        context.HorariosDisponiveis.Add(horario);
        await context.SaveChangesAsync();

        var service = new AgendamentoService(
            context,
            new EmailService()
        );

        var agendamento = new Agendamento
        {
            HorarioDisponivelId = 1,
            NomeResponsavel = "Teste",
            EmailResponsavel = "teste@email.com",
            NomePet = "Thor",
            EspeciePet = "Cachorro",
            Status = "Marcado"
        };

        // Act e Assert
        await Assert.ThrowsAsync<HorarioIndisponivelException>(
            () => service.CriarAsync(agendamento)
        );

        Assert.Empty(context.Agendamentos);
        Assert.Equal("Ocupado", horario.Status);
    }
}