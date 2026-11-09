
using System.Net.Http.Json;
using System.Text.Json;

namespace SafeRouteAI.API.Services;

public sealed class PythonMlHealthClient
{
    private readonly HttpClient _http;

    public PythonMlHealthClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<JsonElement> GetHealthAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync(
            "health", cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<JsonElement>(
            cancellationToken: cancellationToken);
    }
}