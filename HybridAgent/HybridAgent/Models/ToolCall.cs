namespace HybridAgent.Models;

public class ToolCall
{
    public string Name { get; set; } = string.Empty;
    public string ArgumentsJson { get; set; } = string.Empty;
}