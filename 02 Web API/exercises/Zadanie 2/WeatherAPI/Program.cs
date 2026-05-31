var builder = WebApplication.CreateBuilder(args);

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

app.Run();
