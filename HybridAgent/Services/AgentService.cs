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
            tool = "dispense",
            arguments = new
            {
                volume = 15,
                platName = "Plate A1",
                sourceLocation = "A1",
                startColumn = 1,
                endColumn = 12
            }
        }, new JsonSerializerOptions { WriteIndented = true });

        var systemPrompt = $"""
You are controlling a laboratory liquid handler.

When a user asks to dispense liquid:

- Respond with ONLY valid JSON.
- Do NOT include explanation.
- Do NOT wrap in markdown.
- Do NOT include text before or after JSON.
- Property names must match exactly.
- Output must be parseable by System.Text.Json.

Example tool call:

{jsonExample}

Available tools:

{toolsJson}

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