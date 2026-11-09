
using System.Net;
using System.Text;
using System.Text.Json;

namespace SafeRouteAI.API.Services;

public sealed class PythonMlPredictionClient
{
    private readonly HttpClient _http;

    public PythonMlPredictionClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<PythonMlPredictionResult> PredictRouteAsync(
        JsonElement request,
        CancellationToken cancellationToken = default)
    {
        using var content = new StringContent(
            request.GetRawText(),
            Encoding.UTF8,
            "application/json");

        using var response = await _http.PostAsync(
            "predict-route",
            content,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        JsonElement payload;

        try
        {
            using var document = JsonDocument.Parse(body);
            payload = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new HttpRequestException(
                "Python ML returned an invalid JSON response.",
                null,
                HttpStatusCode.BadGateway);
        }

        return new PythonMlPredictionResult(
            (int)response.StatusCode,
            payload);
    }
}

public sealed record PythonMlPredictionResult(
    int StatusCode,
    JsonElement Payload);
