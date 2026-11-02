using ChupinVetAgenda.Controllers;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Services.Exceptions;
using ChupinVetAgenda.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ChupinVetAgenda.Tests.Unit.Controllers;

public class AgendamentosControllerTests
{
    [Fact]
    public async Task PostAgendamento_HorarioIndisponivel_RetornaBadRequest()
    {
        // Arrange
        var serviceMock = new Mock<IAgendamentoService>();

        var loggerMock =
            new Mock<ILogger<AgendamentosController>>();

        var agendamento = new Agendamento
        {
            HorarioDisponivelId = 1,
            NomeResponsavel = "Samantha",
            EmailResponsavel = "samantha@email.com",
            NomePet = "Thor",
            EspeciePet = "Cachorro",
            Status = "Marcado"
        };

        serviceMock
            .Setup(s => s.CriarAsync(
                It.IsAny<Agendamento>()))
            .ThrowsAsync(
                new HorarioIndisponivelException());

        var controller = new AgendamentosController(
            serviceMock.Object,
            loggerMock.Object
        );

        // Act
        var resultado =
            await controller.PostAgendamento(agendamento);

        // Assert
        var badRequest =
            Assert.IsType<BadRequestObjectResult>(
                resultado.Result
            );

        Assert.Equal(
            "Horario indisponivel",
            badRequest.Value
        );

        serviceMock.Verify(
            s => s.CriarAsync(agendamento),
            Times.Once
        );
    }
}