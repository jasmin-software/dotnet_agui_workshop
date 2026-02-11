using System.ComponentModel;

namespace Server.Tools;

internal static class WeatherBackendTool
{
    [Description("Lookup the weather in a location.")]
    public static string GetWeather(string location)
    {
        // In a real implementation, this would call a weather API.
        return $"The weather in {location} is sunny with a high of 25°C.";
    }
}