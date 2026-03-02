using HybridAgent.Tools;

namespace HybridAgent.Services;

public class ToolMetadata
{
    public ITool Tool { get; init; } = default!;
    public List<string> RequiredFields { get; init; } = new();
}