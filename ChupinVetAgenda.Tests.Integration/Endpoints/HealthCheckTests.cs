using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChupinVetAgenda.Tests.Integration.Endpoints;

public class HealthCheckTests
{
    [Fact]
    public async Task HealthCheck_AplicacaoSaudavel_RetornaOk()
    {
        // Arrange
        await using var factory =
            new WebApplicationFactory<Program>();

        var client =
            factory.CreateClient();

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