using System.Net;
using System.Net.Http.Headers;
using ChupinVetAgenda.Tests.Integration.Fixtures;

namespace ChupinVetAgenda.Tests.Integration.Endpoints;

[Collection("IntegrationTests")]
public class HorariosEndpointTests
{
    private readonly IntegrationTestFixture _fixture;

    public HorariosEndpointTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient CriarClienteAutenticado()
    {
        var client = _fixture.Factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                TestAuthHandler.Scheme,
                "usuario-teste"
            );

        return client;
    }

    [Fact]
    public async Task GetHorarios_RequisicaoValida_RetornaOk()
    {
        using var client = CriarClienteAutenticado();

        var response = await client.GetAsync("/api/Horarios");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHorario_IdInexistente_RetornaNotFound()
    {
        using var client = CriarClienteAutenticado();

        var response = await client.GetAsync("/api/Horarios/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetHorarios_SemAutenticacao_RetornaUnauthorized()
    {
        using var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/api/Horarios");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}