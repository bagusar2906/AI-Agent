namespace HybridAgent.Services;

using Tools;
using System.Text.Json;
using System.Text.Json.Nodes;

public class ToolRegistry
{
    private readonly Dictionary<string, ToolMetadata> _tools;

    public ToolRegistry(IEnumerable<ITool> tools)
    {
        _tools = tools.ToDictionary(
            t => t.Name,
            t => new ToolMetadata
            {
                Tool = t,
                RequiredFields = ExtractRequiredFields(t)
            },
            StringComparer.OrdinalIgnoreCase);
    }

    private static List<string> ExtractRequiredFields(ITool tool)
    {
        var schemaNode = JsonNode.Parse(
            JsonSerializer.Serialize(tool.GetSchema()));

        return schemaNode?["function"]?["parameters"]?["required"]
            ?.AsArray()
            .Select(x => x?.ToString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList() ?? new List<string>();
    }

    public IEnumerable<object> GetToolSchemas()
        => _tools.Values.Select(x => x.Tool.GetSchema());

    public List<string> GetMissingFields(string toolName, string argumentsJson)
    {
        if (!_tools.TryGetValue(toolName, out var meta))
            throw new InvalidOperationException("Tool not registered.");

        var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;

        var missing = new List<string>();

        foreach (var field in meta.RequiredFields)
        {
            if (!root.TryGetProperty(field, out var value) ||
                value.ValueKind == JsonValueKind.Null)
            {
                missing.Add(field);
            }
        }

        return missing;
    }

    public async Task<Task<IToolResult>> ExecuteToolAsync(string toolName, string argumentsJson)
    {
        if (!_tools.TryGetValue(toolName, out var meta))
            throw new InvalidOperationException("Tool not registered.");

        var typedArgs = JsonSerializer.Deserialize(
            argumentsJson,
            meta.Tool.ArgumentsType,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (typedArgs == null)
            throw new InvalidOperationException("Invalid arguments.");

        return  meta.Tool.ExecuteFromJsonAsync(typedArgs);
    }
}