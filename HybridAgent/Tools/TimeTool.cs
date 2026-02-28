namespace HybridAgent.Tools;

public class TimeTool : IAgentTool
{
    public string Name => "get_current_time";

    public string Description =>
        "Returns the current server time.";

    public object ParametersSchema => new
    {
        type = "object",
        properties = new { },
        required = new string[] { }
    };

    public Task<string> ExecuteAsync(string argumentsJson)
    {
        return Task.FromResult(DateTime.UtcNow.ToString("O"));
    }
}