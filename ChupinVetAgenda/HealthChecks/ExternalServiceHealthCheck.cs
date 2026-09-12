using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ChupinVetAgenda.HealthChecks;

public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;

    public ExternalServiceHealthCheck(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                "https://www.google.com",
                cancellationToken
            );

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy(
                    "Serviço externo disponível."
                );
            }

            return HealthCheckResult.Unhealthy(
                $"Serviço externo retornou status {(int)response.StatusCode}."
            );
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Serviço externo indisponível.",
                ex
            );
        }
    }
}