namespace HybridAgent.Tools;

public interface ITool
{
    string Name { get; }

    object GetSchema();

    Type ArgumentsType { get; }

    Task<IToolResult> ExecuteFromJsonAsync(object args);
}