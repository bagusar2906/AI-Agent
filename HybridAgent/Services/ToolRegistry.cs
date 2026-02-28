using HybridAgent.Tools;

namespace HybridAgent.Services;

public class ToolRegistry
{
    private readonly IEnumerable<IAgentTool> _tools;

    public ToolRegistry(IEnumerable<IAgentTool> tools)
    {
        _tools = tools;
    }

    public IEnumerable<object> GetToolSchemas()
    {
        return _tools.Select(t => new
        {
            name = t.Name,
            description = t.Description,
            parameters = t.ParametersSchema
        });
    }

    public async Task<string> ExecuteAsync(string name, string argsJson)
    {
        var tool = _tools.FirstOrDefault(t => t.Name == name);

        if (tool == null)
            return "Tool not found.";

        return await tool.ExecuteAsync(argsJson);
    }
}