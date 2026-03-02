namespace HybridAgent.Services;

public class HybridRouter : IAgent
{
    private readonly ToolExecutor _toolExecutor;
    private readonly ChatAssistant _chatAssistant;

    public HybridRouter(
        ToolExecutor toolExecutor,
        ChatAssistant chatAssistant)
    {
        _toolExecutor = toolExecutor;
        _chatAssistant = chatAssistant;
    }

    public IAsyncEnumerable<string> RunAsync(
        string userMessage,
        CancellationToken ct = default)
    {
        if (IsToolIntent(userMessage))
            return _toolExecutor.RunAsync(userMessage, ct);

        return _chatAssistant.RunAsync(userMessage, ct);
    }

    private bool IsToolIntent(string message)
    {
        var keywords = new[]
        {
            "dispense",
            "aspirate",
            "move",
            "plate",
            "column"
        };

        return keywords.Any(k =>
            message.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}