using System.Net.Http.Json;

namespace Blazor_Client.Services;

public interface IAiService
{
    Task<string> SendMessageAsync(string message);
}

public class AiService : IAiService
{
    private readonly HttpClient _http;

    public AiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> SendMessageAsync(string message)
    {
        var payload = new { message };
        using var resp = await _http.PostAsJsonAsync("https://api.example.com/ai", payload);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<AiResponse>();
        return body?.response ?? string.Empty;
    }

    private class AiResponse
    {
        public string response { get; set; }
    }
}
