using NT8Assistant.Tools;

namespace NT8Assistant.Services;

public class ToolMetadata
{
    public ITool Tool { get; init; } = default!;
    public List<string> RequiredFields { get; init; } = new();
}