using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace WeatherAPI;

public sealed class OpenWeatherMapClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OpenWeatherMapClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<OpenWeatherForecastResponse> GetForecastAsync(
        string city,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["OpenWeatherMap:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new OpenWeatherMapConfigurationException(
                "brak klucza api");
        }

        var requestUri = QueryHelpers.AddQueryString("forecast", new Dictionary<string, string?>
        {
            ["q"] = city,
            ["appid"] = apiKey,
            ["units"] = _configuration["OpenWeatherMap:Units"] ?? "metric",
            ["lang"] = _configuration["OpenWeatherMap:Language"] ?? "pl",
            ["cnt"] = _configuration["OpenWeatherMap:ForecastCount"] ?? "8"
        });

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<OpenWeatherMapErrorResponse>(
                cancellationToken: cancellationToken);

            throw new OpenWeatherMapException(
                error?.Message ?? "OpenWeatherMap zwrocil blad podczas pobierania prognozy pogody.",
                response.StatusCode);
        }

        var forecast = await response.Content.ReadFromJsonAsync<OpenWeatherForecastResponse>(
            cancellationToken: cancellationToken);

        return forecast
            ?? throw new OpenWeatherMapException(
                "OpenWeatherMap zwrocil pusta odpowiedz.",
                HttpStatusCode.BadGateway);
    }
}

public sealed class OpenWeatherMapConfigurationException : Exception
{
    public OpenWeatherMapConfigurationException(string message)
        : base(message)
    {
    }
}

public sealed class OpenWeatherMapException : Exception
{
    public OpenWeatherMapException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}

internal sealed record OpenWeatherMapErrorResponse(
    [property: JsonPropertyName("cod")] string Code,
    [property: JsonPropertyName("message")] string Message);
