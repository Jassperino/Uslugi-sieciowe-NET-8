using System.Text.Json.Serialization;

namespace WeatherAPI;

public sealed record OpenWeatherForecastResponse(
    [property: JsonPropertyName("cod")] string Code,
    [property: JsonPropertyName("message")] double Message,
    [property: JsonPropertyName("cnt")] int Count,
    [property: JsonPropertyName("list")] List<OpenWeatherForecastItem> Forecasts,
    [property: JsonPropertyName("city")] OpenWeatherCity City);

public sealed record OpenWeatherForecastItem(
    [property: JsonPropertyName("dt")] long UnixTime,
    [property: JsonPropertyName("main")] OpenWeatherMain Main,
    [property: JsonPropertyName("weather")] List<OpenWeatherCondition> Weather,
    [property: JsonPropertyName("wind")] OpenWeatherWind Wind,
    [property: JsonPropertyName("visibility")] int Visibility,
    [property: JsonPropertyName("pop")] double ProbabilityOfPrecipitation,
    [property: JsonPropertyName("dt_txt")] string DateTimeText);

public sealed record OpenWeatherMain(
    [property: JsonPropertyName("temp")] double Temperature,
    [property: JsonPropertyName("feels_like")] double FeelsLike,
    [property: JsonPropertyName("temp_min")] double TemperatureMin,
    [property: JsonPropertyName("temp_max")] double TemperatureMax,
    [property: JsonPropertyName("pressure")] int Pressure,
    [property: JsonPropertyName("humidity")] int Humidity,
    [property: JsonPropertyName("sea_level")] int? SeaLevel,
    [property: JsonPropertyName("grnd_level")] int? GroundLevel);

public sealed record OpenWeatherCondition(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("main")] string Main,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("icon")] string Icon);

public sealed record OpenWeatherWind(
    [property: JsonPropertyName("speed")] double Speed,
    [property: JsonPropertyName("deg")] int Degrees,
    [property: JsonPropertyName("gust")] double? Gust);

public sealed record OpenWeatherCity(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("coord")] OpenWeatherCoordinates Coordinates,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("timezone")] int Timezone,
    [property: JsonPropertyName("sunrise")] long Sunrise,
    [property: JsonPropertyName("sunset")] long Sunset);

public sealed record OpenWeatherCoordinates(
    [property: JsonPropertyName("lat")] double Latitude,
    [property: JsonPropertyName("lon")] double Longitude);
