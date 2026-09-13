using System.Net;
using ChupinVetAgenda.Tests.Integration.Fixtures;

namespace ChupinVetAgenda.Tests.Integration.Endpoints;

[Collection("IntegrationTests")]
public class HorariosEndpointTests
{
    private readonly IntegrationTestFixture _fixture;

    public HorariosEndpointTests(
        IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetHorarios_RequisicaoValida_RetornaOk()
    {
        // Arrange
        var client = _fixture.Client;

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
        var client = _fixture.Client;

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