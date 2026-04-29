using System.Net.Http.Json;
using Polly;
using Polly.Retry;
using Softcoinp.ERP.Domain.External;
using Softcoinp.ERP.Domain.Interfaces;

namespace Softcoinp.ERP.Infrastructure.External;

/// <summary>
/// Implementation of the Core integration client using HttpClient and Polly for resilience.
/// </summary>
public class CoreIntegrationClient : ICoreIntegrationClient
{
    private readonly HttpClient _httpClient;
    private readonly ITenantResolver _tenantResolver;
    private readonly AsyncRetryPolicy _retryPolicy;

    public CoreIntegrationClient(HttpClient httpClient, ITenantResolver tenantResolver)
    {
        _httpClient = httpClient;
        _tenantResolver = tenantResolver;

        // Configure Polly retry policy: 3 retries with exponential backoff
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private async Task PrepareClientAsync()
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        if (tenant == null || string.IsNullOrEmpty(tenant.CoreApiUrl))
        {
            throw new InvalidOperationException("Current tenant or Core API URL not found.");
        }

        _httpClient.BaseAddress = new Uri(tenant.CoreApiUrl);
        
        // Propagate X-Tenant-Id
        if (!_httpClient.DefaultRequestHeaders.Contains("X-Tenant-Id"))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenant.Subdomain);
        }
    }

    public async Task<CoreUnitDto?> GetUnitByIdAsync(Guid unitId)
    {
        await PrepareClientAsync();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"/api/units/{unitId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CoreUnitDto>();
        });
    }

    public async Task<IEnumerable<CoreUnitDto>> GetAllUnitsAsync()
    {
        await PrepareClientAsync();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync("/api/units");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<CoreUnitDto>>() 
                   ?? Enumerable.Empty<CoreUnitDto>();
        });
    }
}
