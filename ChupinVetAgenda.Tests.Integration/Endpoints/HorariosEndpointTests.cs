using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChupinVetAgenda.Tests.Integration.Endpoints;

public class HorariosEndpointTests
{
    [Fact]
    public async Task GetHorarios_RequisicaoValida_RetornaOk()
    {
        // Arrange
        await using var factory =
            new WebApplicationFactory<Program>();

        var client =
            factory.CreateClient();

        // Act
        var response =
            await client.GetAsync("/api/Horarios");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }

    [Fact]
    public async Task GetHorario_IdInexistente_RetornaNotFound()
    {
        // Arrange
        await using var factory =
            new WebApplicationFactory<Program>();

        var client =
            factory.CreateClient();

        // Act
        var response =
            await client.GetAsync(
                "/api/Horarios/999999"
            );

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    } 
}