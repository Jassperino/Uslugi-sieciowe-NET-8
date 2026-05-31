using System.Net;
using WeatherAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<OpenWeatherMapClient>(client =>
{
    var baseUrl = builder.Configuration["OpenWeatherMap:BaseUrl"]
        ?? "https://api.openweathermap.org/data/2.5/";

    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

app.UseHttpsRedirection();

var windDirections = new[]
{
    "N",
    "NE",
    "E",
    "SE",
    "S",
    "SW",
    "W",
    "NW"
};

app.MapGet("/temperature", () =>
{
    var temperatureC = Random.Shared.Next(-30, 46);

    return Results.Ok(new { temperatureC });
});

app.MapGet("/wind-direction", () =>
{
    var windDirection = windDirections[Random.Shared.Next(windDirections.Length)];

    return Results.Ok(new { windDirection });
});

app.MapGet("/weather/{city}", async Task<IResult> (
    string city,
    OpenWeatherMapClient weatherApi,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(city))
    {
        return Results.BadRequest(new { message = "Podaj nazwe miasta." });
    }

    try
    {
        var forecast = await weatherApi.GetForecastAsync(city, cancellationToken);

        return Results.Ok(forecast);
    }
    catch (OpenWeatherMapConfigurationException exception)
    {
        return Results.Problem(
            title: "Brak konfiguracji OpenWeatherMap",
            detail: exception.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
    catch (OpenWeatherMapException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
    {
        return Results.NotFound(new { message = $"Nie znaleziono miasta: {city}." });
    }
    catch (OpenWeatherMapException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
    {
        return Results.Problem(
            title: "Blad autoryzacji OpenWeatherMap",
            detail: "Sprawdz wartosc OpenWeatherMap:ApiKey w konfiguracji aplikacji.",
            statusCode: StatusCodes.Status502BadGateway);
    }
    catch (OpenWeatherMapException exception)
    {
        return Results.Problem(
            title: "Blad API pogodowego",
            detail: exception.Message,
            statusCode: StatusCodes.Status502BadGateway);
    }
});

app.Run();
