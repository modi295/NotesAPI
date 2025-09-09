using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NotesAPI.Repositories;

public class GroqBotService : IBotService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GroqBotService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Groq:ApiKey"]!;
    }

    public async Task<string> GetResponseAsync(string message)
    {
        var payload = new
        {
            model = "llama3-8b-8192",
            messages = new[]
            {
                new { role = "system", content = "You are a helpful chatbot. Always reply briefly in 1 or 2 sentences." },
                new { role = "user", content = message }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
                  .GetProperty("choices")[0]
                  .GetProperty("message")
                  .GetProperty("content")
                  .GetString()!;
    }
}
