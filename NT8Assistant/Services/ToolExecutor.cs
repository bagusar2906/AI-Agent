using System.Runtime.CompilerServices;
using System.Text.Json;
using NT8Assistant.Helpers;

namespace NT8Assistant.Services;

public interface IAgent
{
    IAsyncEnumerable<string> RunAsync(
        string userMessage,
        CancellationToken ct = default);

    string? GenerateSuggestion(string input);
}
public class ToolExecutor(OllamaService ollama, ToolRegistry registry) : IAgent
{
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public async IAsyncEnumerable<string> RunAsync(
        string userMessage,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var toolsJson = JsonSerializer.Serialize(
            registry.GetToolSchemas(),
            _jsonOptions);

        var jsonExample1 = JsonSerializer.Serialize(new
        {
            tool = "dispense",
            arguments = new[] { new
            {
                volume = 15,
                station = "Station 1",
                sourceLocation = "A1",
                startColumn = 1,
                endColumn = 3
            }}
        }, _jsonOptions);

        var jsonExample2 = JsonSerializer.Serialize(new
        {
            tool = "dispense",
            arguments = new[]
            {
                new
                {
                    volume = 10,
                    station = "Protein A1",
                    sourceLocation = "A1",
                    startColumn = 1,
                    endColumn = 3
                },
                new
                {
                    volume = 5,
                    station = "Station 1",
                    sourceLocation = "Station 2",
                    startColumn = 2,
                    endColumn = 4
                }
            }
        }, _jsonOptions);

        // 🔹 1. Build system prompt
        var systemPrompt = $"""
You are controlling a laboratory liquid handler.

When a user asks to dispense liquid:

- You MUST call the "dispense" tool.
- Do NOT respond with raw JSON.
- Do NOT respond with explanation.
- Return a function call.

The dispense tool expects:

{toolsJson}

Rules:
- "items" must always be an array.
- Even for a single dispense, wrap it inside the "items" array.
- Never return a single object.
- Never return a raw array.
- Do NOT reuse previous values.
- Do NOT guess missing values.

Example 1:
User: Dispense 15uL from A1 to Station 1 columns 1 to 3.
Tool arguments:

{jsonExample1}

Example 2:
User: Dispense 10uL from Protein A1 to Station 1 columns 1 to 3 and Fragment 5uL from Station 2 to Station 1 columns 2 to 4.
Tool arguments:

{jsonExample2}

Otherwise respond normally.

""";

        // Simulate missing fields
        if (userMessage.ToLower().Contains("simulate"))
        {
            var message =
                AgentMessageBuilder.BuildValidationMessage(new List<string> { "Volume", "Source Location" },
                    userMessage);
            yield return JsonSerializer.Serialize(message, JsonHelper.Default);
            yield break;
        }
        // 🔹 2. First reasoning passes
        var firstResponse =
            await ollama.GenerateAsync(systemPrompt + "\nUser: " + userMessage);

        // 🔹 3. Try to interpret as a tool call
        var toolCall = await EnsureValidToolCallAsync(firstResponse);

        if (toolCall != null)
        {
            // 🔹 4. Validate required parameters dynamically
            var missing = registry.GetMissingFields(
                toolCall.Value.Tool,
                toolCall.Value.ArgumentsJson);
            
            if (missing.Any())
            {
                var message = AgentMessageBuilder.BuildValidationMessage(missing, userMessage);
                yield return JsonSerializer.Serialize(message, JsonHelper.Default);
                yield break;
            }

            // 🔹 5. Execute tool
            var toolResult = await registry.ExecuteToolAsync(
                toolCall.Value.Tool,
                toolCall.Value.ArgumentsJson);

            // 🔹 IMPORTANT: Serialize result to JSON
            var toolResultJson = JsonSerializer.Serialize(toolResult.Result, JsonHelper.Default);
            yield return toolResultJson;
        }
        else
        {
            // 🔹 7. If no tool calls, stream a normal response
            await foreach (var chunk in ollama.StreamAsync(userMessage, ct))
                yield return chunk;
        }
    }

    public string? GenerateSuggestion(string input)
    {
        throw new NotImplementedException();
    }

    private bool TryParseToolCall(string? text,
        out string tool,
        out string args)
    {
        tool = "";
        args = "";
        if (text == null)
            return false;

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
    
    private string BuildClarificationQuestion(IEnumerable<string> missing)
    {
        return "I need additional information before proceeding: "
               + string.Join(", ", missing)
               + ". Please provide the missing values.";
    }
    private async Task<(string Tool, string ArgumentsJson)?> EnsureValidToolCallAsync(string? response)
    {
        const int maxRetries = 2;

        string? current = CleanJson(response);

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

            current = CleanJson(await ollama.GenerateAsync(fixPrompt));
        }

        return null;
    }

    private string? CleanJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        return text
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();
    }
}