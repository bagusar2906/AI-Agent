using System.Runtime.CompilerServices;
using System.Text.Json;

namespace NT8Assistant.Services;

public class OllamaService
{
    private readonly HttpClient _http;
    private readonly string? _model;

    public OllamaService(HttpClient http, IConfiguration config)
    {
        _http = http;
        var baseUrl = config["Ollama:BaseUrl"];
        _model = config["Ollama:Model"];
        _http.BaseAddress = new Uri(baseUrl);
        if (config.GetValue<bool>("Ollama:UseBasicAuth"))
            _http.DefaultRequestHeaders.Add("Authorization", "Bearer " + config["Ollama:ApiKey"]);

    }

    public async Task<string?> GenerateAsync(string userPrompt)
    {
        var response = await _http.PostAsJsonAsync("/api/generate", new
        {
            model = _model,
            prompt = userPrompt,
            stream = false
        });

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("response").GetString();
    }

    public async IAsyncEnumerable<string> StreamAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
       
        var requestBody = new
        {
            model = _model,
            prompt = prompt,
            stream = true
        };

        var response = await _http.PostAsJsonAsync(
            "/api/generate",
            requestBody,
            ct);

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            using var doc = JsonDocument.Parse(line);

            if (doc.RootElement.TryGetProperty("response", out var token))
            {
                yield return token.GetString()!;
            }

            if (doc.RootElement.TryGetProperty("done", out var doneProp) &&
                doneProp.GetBoolean())
            {
                yield break;
            }
        }
    }
}