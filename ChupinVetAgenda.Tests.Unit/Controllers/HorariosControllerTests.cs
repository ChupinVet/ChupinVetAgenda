using ChupinVetAgenda.Controllers;
using ChupinVetAgenda.Data;
using ChupinVetAgenda.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ChupinVetAgenda.Tests.Unit.Controllers;

public class HorariosControllerTests
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
    public async Task GetHorario_HorarioExistente_RetornaHorario()
    {
        // Arrange
        var context = CriarContexto();

        var logger =
            new Mock<ILogger<HorariosController>>();

        var horario = new HorarioDisponivel
        {
            Id = 1,
            DataHora = new DateTime(2026, 9, 20, 10, 0, 0),
            TipoAtendimento = "Consulta",
            Status = "Disponivel"
        };

        context.HorariosDisponiveis.Add(horario);
        await context.SaveChangesAsync();

        var controller =
            new HorariosController(
                context,
                logger.Object
            );

        // Act
        var resultado =
            await controller.GetHorario(1);

        // Assert
        var horarioRetornado =
            Assert.IsType<HorarioDisponivel>(
                resultado.Value
            );

        Assert.Equal(1, horarioRetornado.Id);
        Assert.Equal(
            "Consulta",
            horarioRetornado.TipoAtendimento
        );
        Assert.Equal(
            "Disponivel",
            horarioRetornado.Status
        );
    }
}