using ChupinVetAgenda.Data;
using ChupinVetAgenda.Services;
using ChupinVetAgenda.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ChupinVetAgenda.Services.Interfaces;
using ChupinVetAgenda.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/chupinvet-.txt",
        rollingInterval: RollingInterval.Day
    )
    .CreateLogger();

builder.Host.UseSerilog();

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService("ChupinVetAgenda"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });

// EF Core + Oracle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString(
        "OracleConnection"
        ))
);

// Health Checks
builder.Services.AddHttpClient<ExternalServiceHealthCheck>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("Oracle")
    .AddCheck<ExternalServiceHealthCheck>("ServicoExterno");

//Controllers
builder.Services.AddControllers();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Email
builder.Services.AddScoped<EmailService>();

//Services - Agendamento
builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();

//Services - Horário
builder.Services.AddScoped<IHorarioService, HorarioService>();

var app = builder.Build();

// Correlation ID
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                        ?? Guid.NewGuid().ToString();

    context.Response.Headers["X-Correlation-ID"] = correlationId;

    using (Serilog.Context.LogContext.PushProperty(
               "CorrelationId", correlationId))
    {
        await next();
    }
});

// Logging das requisições
app.UseSerilogRequestLogging();

//Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

public partial class Program { }