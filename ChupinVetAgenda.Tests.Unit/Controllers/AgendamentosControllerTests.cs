using ChupinVetAgenda.Controllers;
using ChupinVetAgenda.Data;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ChupinVetAgenda.Tests.Unit.Controllers;

public class AgendamentosControllerTests
{
    private AppDbContext CriarContexto()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(
                    databaseName: Guid.NewGuid().ToString()
                )
                .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task PostAgendamento_HorarioIndisponivel_RetornaBadRequest()
    {
        // Arrange
        var context = CriarContexto();

        var logger =
            new Mock<ILogger<AgendamentosController>>();

        var emailService =
            new EmailService();

        var horario = new HorarioDisponivel
        {
            Id = 1,
            DataHora = new DateTime(2026, 9, 20, 10, 0, 0),
            TipoAtendimento = "Consulta",
            Status = "Ocupado"
        };

        context.HorariosDisponiveis.Add(horario);
        await context.SaveChangesAsync();

        var controller =
            new AgendamentosController(
                context,
                emailService,
                logger.Object
            );

        var agendamento = new Agendamento
        {
            HorarioDisponivelId = 1,
            NomeResponsavel = "Samantha",
            EmailResponsavel = "samantha@email.com",
            NomePet = "Thor",
            EspeciePet = "Cachorro",
            Status = "Marcado"
        };

        // Act
        var resultado =
            await controller.PostAgendamento(
                agendamento
            );

        // Assert
        var badRequest =
            Assert.IsType<BadRequestObjectResult>(
                resultado.Result
            );

        Assert.Equal(
            "Horario indisponivel",
            badRequest.Value
        );
    }    
}