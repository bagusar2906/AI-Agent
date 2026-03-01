using System.Text.Json.Nodes;
using HybridAgent.Tools;

namespace HybridAgent.Services;

using System.Text.Json;

public class ToolRegistry
{
    private readonly Dictionary<string, IAgentTool> _tools;

    public ToolRegistry(IEnumerable<IAgentTool> tools)
    {
        _tools = tools.ToDictionary(
            t => t.Name,
            t => t,
            StringComparer.OrdinalIgnoreCase);
    }

    // 🔹 Expose tool schemas to LLM
    public IEnumerable<object> GetToolSchemas()
    {
        return _tools.Values.Select(t => new
        {
            name = t.Name,
            description = t.Description,
            parameters = t.ParametersSchema
        });
    }

    // 🔹 Execute tool safely
    public async Task<string> ExecuteAsync(string toolName, string argsJson)
    {
        if (!_tools.TryGetValue(toolName, out var tool))
            throw new InvalidOperationException(
                $"Tool '{toolName}' not registered.");

        return await tool.ExecuteAsync(argsJson);
    }

    // 🔹 Dynamic schema validation
    // Field is missing if:
    //   - It does not exist
    //   - OR it exists but value is null
    public List<string> GetMissingFields(string toolName, string argumentsJson)
    {
        if (!_tools.TryGetValue(toolName, out var tool))
            throw new InvalidOperationException($"Tool '{toolName}' not registered.");

        var schemaNode = JsonNode.Parse(
            JsonSerializer.Serialize(tool.ParametersSchema));

        var requiredFields = new List<string?>();

        if (schemaNode is JsonObject schemaObj &&
            schemaObj["required"] is JsonArray requiredArray)
        {
            requiredFields = requiredArray
                .Select(r => r?.ToString())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList();
        }

        var argsDoc = JsonDocument.Parse(argumentsJson);
        var argsRoot = argsDoc.RootElement;

        var missing = new List<string>();

        foreach (var field in requiredFields)
        {
            if (!argsRoot.TryGetProperty(field!, out var value))
            {
                // Completely absent
                missing.Add(field!);
                continue;
            }

            if (value.ValueKind == JsonValueKind.Null)
            {
                // Present but explicitly null
                missing.Add(field!);
            }
        }

        return missing;
    }

}