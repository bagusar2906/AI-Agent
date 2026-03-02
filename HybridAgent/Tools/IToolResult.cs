namespace HybridAgent.Tools;

public interface IToolResult
{
    bool Success { get; }
    string ToolName { get; }
    string Message { get; }
    object? Data { get; }
    string? ErrorCode { get; }
}