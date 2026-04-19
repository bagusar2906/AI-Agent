using System.Runtime.CompilerServices;

namespace NT8Assistant.Services;

public class ChatAssistant(OllamaService ollama) : IAgent
{
    public async IAsyncEnumerable<string> RunAsync(
        string userMessage,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var chunk in ollama.StreamAsync(userMessage, ct))
            yield return chunk;
    }

    public string? GenerateSuggestion(string input)
    {
        throw new NotImplementedException();
    }
}