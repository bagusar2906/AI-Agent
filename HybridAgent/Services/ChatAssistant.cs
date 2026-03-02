using System.Runtime.CompilerServices;

namespace HybridAgent.Services;

public class ChatAssistant : IAgent
{
    private readonly OllamaService _ollama;

    public ChatAssistant(OllamaService ollama)
    {
        _ollama = ollama;
    }

    public async IAsyncEnumerable<string> RunAsync(
        string userMessage,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var chunk in _ollama.StreamAsync(userMessage, ct))
            yield return chunk;
    }
}