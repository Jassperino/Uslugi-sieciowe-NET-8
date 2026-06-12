using Microsoft.OpenApi;
using System.Net;
using WeatherAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Weather API",
        Version = "v1",
        Description = "Minimal API for weather forecasts and a collection of cities."
    });
});

builder.Services.AddHttpClient<OpenWeatherMapClient>(client =>
{
    var baseUrl = builder.Configuration["OpenWeatherMap:BaseUrl"]
        ?? "https://api.openweathermap.org/data/2.5/";

    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/api/error");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API v1");
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

var cities = new Dictionary<int, CityResponse>();
var citiesLock = new object();
var nextCityId = 0;

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

//GET /temperature
app.MapGet("/temperature", () =>
{
    var temperatureC = Random.Shared.Next(-30, 46);

    return Results.Ok(new { temperatureC });
});

//GET /wind-direction
app.MapGet("/wind-direction", () =>
{
    var windDirection = windDirections[Random.Shared.Next(windDirections.Length)];

    return Results.Ok(new { windDirection });
})
.WithTags("Weather");

//POST /cities
app.MapPost("/cities", (AddCityRequest request) =>
{
    var cityName = request.Name?.Trim();

    if (string.IsNullOrWhiteSpace(cityName))
    {
        return Results.BadRequest(new { message = "City name is required." });
    }

    lock (citiesLock)
    {
        if (cities.Values.Any(city =>
                string.Equals(city.Name, cityName, StringComparison.OrdinalIgnoreCase)))
        {
            return Results.Conflict(new { message = $"City '{cityName}' already exists." });
        }

        var city = new CityResponse(++nextCityId, cityName);
        cities.Add(city.Id, city);

        return Results.Created($"/cities/{city.Id}", city);
    }
})
.WithName("AddCity")
.WithTags("Cities")
.Accepts<AddCityRequest>("application/json")
.Produces<CityResponse>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status409Conflict);

//GET /cities
app.MapGet("/cities", () =>
{
    CityResponse[] cityList;

    lock (citiesLock)
    {
        cityList = cities.Values
            .OrderBy(city => city.Id)
            .ToArray();
    }

    return Results.Ok(cityList);
})
.WithName("GetCities")
.WithTags("Cities")
.Produces<CityResponse[]>(StatusCodes.Status200OK);

//GET /cities/{id}
app.MapGet("/cities/{id:int}", (int id) =>
{
    lock (citiesLock)
    {
        return cities.TryGetValue(id, out var city)
            ? Results.Ok(city)
            : Results.NotFound(new { message = $"City with id {id} was not found." });
    }
})
.WithName("GetCityById")
.WithTags("Cities")
.Produces<CityResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

//PUT /cities/{id}
app.MapPut("/cities/{id:int}", (int id, UpdateCityRequest request) =>
{
    var cityName = request.Name?.Trim();

    if (string.IsNullOrWhiteSpace(cityName))
    {
        return Results.BadRequest(new { message = "City name is required." });
    }

    lock (citiesLock)
    {
        if (!cities.ContainsKey(id))
        {
            return Results.NotFound(new { message = $"City with id {id} was not found." });
        }

        if (cities.Values.Any(city =>
                city.Id != id &&
                string.Equals(city.Name, cityName, StringComparison.OrdinalIgnoreCase)))
        {
            return Results.Conflict(new { message = $"City '{cityName}' already exists." });
        }

        var updatedCity = new CityResponse(id, cityName);
        cities[id] = updatedCity;

        return Results.Ok(updatedCity);
    }
})
.WithName("UpdateCity")
.WithTags("Cities")
.Accepts<UpdateCityRequest>("application/json")
.Produces<CityResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status409Conflict);

//DELETE /cities/{id}
app.MapDelete("/cities/{id:int}", (int id) =>
{
    lock (citiesLock)
    {
        return cities.Remove(id)
            ? Results.NoContent()
            : Results.NotFound(new { message = $"City with id {id} was not found." });
    }
})
.WithName("DeleteCity")
.WithTags("Cities")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

//GET /weather/{city}
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

app.MapControllers();

app.Run();

public sealed record AddCityRequest(string? Name);

public sealed record UpdateCityRequest(string? Name);

public sealed record CityResponse(int Id, string Name);
