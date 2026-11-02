using ChupinVetAgenda.Controllers;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ChupinVetAgenda.Tests.Unit.Controllers;

public class HorariosControllerTests
{
    [Fact]
    public async Task GetHorario_HorarioExistente_RetornaHorario()
    {
        // Arrange
        var serviceMock = new Mock<IHorarioService>();
        var loggerMock = new Mock<ILogger<HorariosController>>();

        var horario = new HorarioDisponivel
        {
            Id = 1,
            DataHora = new DateTime(2026, 9, 20, 10, 0, 0),
            TipoAtendimento = "Consulta",
            Status = "Disponivel"
        };

        serviceMock
            .Setup(s => s.BuscarPorIdAsync(1))
            .ReturnsAsync(horario);

        var controller = new HorariosController(
            serviceMock.Object,
            loggerMock.Object
        );

        // Act
        var resultado = await controller.GetHorario(1);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado.Result);

        var horarioRetornado =
            Assert.IsType<HorarioDisponivel>(ok.Value);

        Assert.Equal(1, horarioRetornado.Id);
        Assert.Equal("Consulta", horarioRetornado.TipoAtendimento);
        Assert.Equal("Disponivel", horarioRetornado.Status);

        serviceMock.Verify(s => s.BuscarPorIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetHorario_HorarioInexistente_RetornaNotFound()
    {
        // Arrange
        var serviceMock = new Mock<IHorarioService>();
        var loggerMock = new Mock<ILogger<HorariosController>>();

        serviceMock
            .Setup(s => s.BuscarPorIdAsync(999))
            .ReturnsAsync((HorarioDisponivel?)null);

        var controller = new HorariosController(
            serviceMock.Object,
            loggerMock.Object
        );

        // Act
        var resultado = await controller.GetHorario(999);

        // Assert
        Assert.IsType<NotFoundResult>(resultado.Result);

        serviceMock.Verify(s => s.BuscarPorIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task DeleteHorario_HorarioOcupado_RetornaBadRequest()
    {
        // Arrange
        var serviceMock = new Mock<IHorarioService>();
        var loggerMock = new Mock<ILogger<HorariosController>>();

        serviceMock
            .Setup(s => s.ExcluirAsync(1))
            .ThrowsAsync(new InvalidOperationException(
                "Nao e possivel remover um horario ocupado."));

        var controller = new HorariosController(
            serviceMock.Object,
            loggerMock.Object
        );

        // Act
        var resultado = await controller.DeleteHorario(1);

        // Assert
        var badRequest =
            Assert.IsType<BadRequestObjectResult>(resultado);

        Assert.Equal(
            "Nao e possivel remover um horario ocupado.",
            badRequest.Value
        );

        serviceMock.Verify(s => s.ExcluirAsync(1), Times.Once);
    }
}