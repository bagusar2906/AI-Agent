namespace HybridAgent.Tools;

public interface IAgentTool
{
    string Name { get; }
    string Description { get; }
    object ParametersSchema { get; }  // JSON schema object
    Task<string> ExecuteAsync(string argumentsJson);

}