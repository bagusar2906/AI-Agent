using System.Text;
using System.Text.Json;

namespace HybridAgent.Services;

public class OllamaService
{
    private readonly HttpClient _http;
    private readonly string? _model;

    public OllamaService(HttpClient http, IConfiguration config)
    {
        _http = http;
        var baseUrl = config["Ollama:BaseUrl"];
        _model = config["Ollama:Model"];
        _http.BaseAddress = new Uri(baseUrl!);
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        var response = await _http.PostAsJsonAsync("/api/generate", new
        {
            model = _model,
            prompt = prompt,
            stream = false
        });

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("response").GetString();
    }

    public async IAsyncEnumerable<string> StreamAsync(string prompt, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/generate");
        request.Content = new StringContent(JsonSerializer.Serialize(new
        {
            model = _model,
            prompt = prompt,
            stream = true
        }), Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(line))
                yield return line;
        }
    }
}