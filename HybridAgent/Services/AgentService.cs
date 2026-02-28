using System.Runtime.CompilerServices;
using System.Text.Json;
using HybridAgent.Services;

public class AgentService
{
    private readonly OllamaService _ollama;
    private readonly ToolRegistry _registry;

    public AgentService(OllamaService ollama, ToolRegistry registry)
    {
        _ollama = ollama;
        _registry = registry;
    }

    public async IAsyncEnumerable<string> RunAsync(
        string userMessage,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var toolsJson = JsonSerializer.Serialize(
            _registry.GetToolSchemas(),
            new JsonSerializerOptions { WriteIndented = true });

        var jsonExample = JsonSerializer.Serialize(new
        {
            tool = "tool_name",
            arguments = new { }
        }, new JsonSerializerOptions { WriteIndented = true });

        var systemPrompt = $"""
You are an AI assistant.

You have access to the following tools:

{toolsJson}

If a tool is needed, respond ONLY in valid JSON format like this:

{jsonExample}

Otherwise respond normally.
""";

        var firstResponse =
            await _ollama.GenerateAsync(systemPrompt + "\nUser: " + userMessage);

        if (TryParseToolCall(firstResponse, out var toolName, out var argsJson))
        {
            var toolResult = await _registry.ExecuteAsync(toolName, argsJson);

            var finalPrompt = $"""
User question:
{userMessage}

Tool result:
{toolResult}

Now provide the final answer.
""";

            await foreach (var chunk in _ollama.StreamAsync(finalPrompt, ct))
                yield return chunk;
        }
        else
        {
            await foreach (var chunk in _ollama.StreamAsync(userMessage, ct))
                yield return chunk;
        }
    }

    private bool TryParseToolCall(string text,
        out string tool,
        out string args)
    {
        tool = "";
        args = "";

        try
        {
            var doc = JsonDocument.Parse(text);

            tool = doc.RootElement.GetProperty("tool").GetString()!;
            args = doc.RootElement.GetProperty("arguments").GetRawText();

            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}