using ChupinVetAgenda.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace ChupinVetAgenda.Tests.Unit.Middlewares;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ExcecaoInesperada_Retorna500()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();

        RequestDelegate next = _ =>
            throw new Exception("Erro de teste");

        var middleware = new ExceptionMiddleware(
            next,
            loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            context.Response.StatusCode
        );

        Assert.StartsWith(
            "application/json",
            context.Response.ContentType
        );

        context.Response.Body.Position = 0;

        using var reader = new StreamReader(context.Response.Body);
        var resposta = await reader.ReadToEndAsync();

        Assert.Contains("Erro interno do servidor", resposta);
        Assert.DoesNotContain("Erro de teste", resposta);
    }
}