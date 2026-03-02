using System.Runtime.CompilerServices;
using System.Text.Json;
using HybridAgent.Helpers;

namespace HybridAgent.Services;

public interface IAgent
{
    IAsyncEnumerable<string> RunAsync(
        string userMessage,
        CancellationToken ct = default);
}
public class ToolExecutor : IAgent
{
    private readonly OllamaService _ollama;
    private readonly ToolRegistry _registry;
    private readonly JsonSerializerOptions _jsonOptions;

    public ToolExecutor(OllamaService ollama, ToolRegistry registry)
    {
        _ollama = ollama;
        _registry = registry;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    public async IAsyncEnumerable<string> RunAsync(
        string userMessage,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var toolsJson = JsonSerializer.Serialize(
            _registry.GetToolSchemas(),
            _jsonOptions);

        var jsonExample = JsonSerializer.Serialize(new
        {
            tool = "dispense",
            arguments = new
            {
                volume = 15,
                plateName = "Plate A1",
                sourceLocation = "A1",
                startColumn = 1,
                endColumn = 12
            }
        }, _jsonOptions);

        // 🔹 1. Build system prompt
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

        // 🔹 2. First reasoning pass
        var firstResponse =
            await _ollama.GenerateAsync(systemPrompt + "\nUser: " + userMessage);

        // 🔹 3. Try to interpret as tool call
        var toolCall = await EnsureValidToolCallAsync(firstResponse);

        if (toolCall != null)
        {
            // 🔹 4. Validate required parameters dynamically
            var missing = _registry.GetMissingFields(
                toolCall.Value.Tool,
                toolCall.Value.ArgumentsJson);

            if (missing.Any())
            {
                yield return BuildClarificationQuestion(missing);
                yield break;
            }

            // 🔹 5. Execute tool
            var toolResult = await _registry.ExecuteToolAsync(
                toolCall.Value.Tool,
                toolCall.Value.ArgumentsJson);

            // 🔹 IMPORTANT: Serialize result to JSON
            var toolResultJson = JsonSerializer.Serialize(toolResult.Result, JsonHelper.Default);
            yield return toolResultJson;
        }
        else
        {
            // 🔹 7. If no tool call, stream normal response
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

            if (!doc.RootElement.TryGetProperty("tool", out var toolProp))
                return false;

            if (!doc.RootElement.TryGetProperty("arguments", out var argsProp))
                return false;

            tool = toolProp.GetString()!;
            args = argsProp.GetRawText();

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string BuildClarificationQuestion(List<string> missing)
    {
        return "I need additional information before proceeding: "
               + string.Join(", ", missing)
               + ". Please provide the missing values.";
    }
    private async Task<(string Tool, string ArgumentsJson)?> EnsureValidToolCallAsync(string response)
    {
        const int maxRetries = 2;

        string current = CleanJson(response);

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            if (TryParseToolCall(current, out var tool, out var args))
            {
                return (tool, args);
            }

            if (attempt == maxRetries)
                return null;

            // 🔥 Self-healing correction prompt
            var fixPrompt = $"""
The previous response was intended to be a JSON tool call
but it was not valid or parseable.

Return ONLY valid JSON.
Do NOT include explanation.
Do NOT include markdown.
Do NOT include text before or after JSON.

Fix this:

{current}
""";

            current = CleanJson(await _ollama.GenerateAsync(fixPrompt));
        }

        return null;
    }

    private string CleanJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        return text
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();
    }
}