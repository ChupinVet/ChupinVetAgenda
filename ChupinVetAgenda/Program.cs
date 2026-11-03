using ChupinVetAgenda.Data;
using ChupinVetAgenda.Services;
using ChupinVetAgenda.HealthChecks;
using ChupinVetAgenda.Services.Interfaces;
using ChupinVetAgenda.Services.Implementations;
using ChupinVetAgenda.Middlewares;
using ChupinVetAgenda.Repositories.Interfaces;
using ChupinVetAgenda.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text;
using ChupinVetAgenda.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;

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

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "A chave JWT nao foi configurada.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

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
    options.UseOracle(
        builder.Configuration.GetConnectionString("OracleConnection")
    )
);

// Health Checks
builder.Services.AddHttpClient<ExternalServiceHealthCheck>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("Oracle")
    .AddCheck<ExternalServiceHealthCheck>("ServicoExterno");

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OrderActionsBy(api =>
    {
        var controller = api.ActionDescriptor.RouteValues["controller"];

        var prioridade = controller switch
        {
            "Auth" => "0",
            "Agendamentos" => "1",
            "Horarios" => "2",
            _ => "9"
        };

        return prioridade + "_" + api.RelativePath;
    });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT gerado no login."
    });
    
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] =
                new List<string>()
        });
});

// Email
builder.Services.AddScoped<EmailService>();

// Services
builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
builder.Services.AddScoped<IHorarioService, HorarioService>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();


// Repositories
builder.Services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddScoped<IHorarioRepository, HorarioRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

var app = builder.Build();

// Tratamento global de exceções
app.UseMiddleware<ExceptionMiddleware>();

// Correlation ID
app.Use(async (context, next) =>
{
    var correlationId =
        context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
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

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Autenticação e autorização JWT
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }