namespace HybridAgent.Tools;

public interface IAgentTool
{
    string Name { get; }

    object GetSchema();

    Type ArgumentsType { get; }

    Task<string> ExecuteUntypedAsync(object args);
}