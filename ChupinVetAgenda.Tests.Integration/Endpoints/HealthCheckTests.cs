using System.Net;
using ChupinVetAgenda.Tests.Integration.Fixtures;

namespace ChupinVetAgenda.Tests.Integration.Endpoints;

[Collection("IntegrationTests")]
public class HealthCheckTests
{
    private readonly IntegrationTestFixture _fixture;

    public HealthCheckTests(
        IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HealthCheck_AplicacaoSaudavel_RetornaOk()
    {
        // Arrange
        var client = _fixture.Client;

        // Act
        var response =
            await client.GetAsync("/health");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }
}