namespace HybridAgent.Tools;

public class ToolResult : IToolResult
{
    public bool Success { get; init; }
    public string ToolName { get; init; } = "";
    public string Message { get; init; } = "";
    public object? Data { get; init; }
    public string? ErrorCode { get; init; }

    public static ToolResult Ok(string toolName, string message, object? data = null)
        => new()
        {
            Success = true,
            ToolName = toolName,
            Message = message,
            Data = data
        };

    public static  ToolResult Fail(string toolName, string message, string errorCode)
        => new()
        {
            Success = false,
            ToolName = toolName,
            Message = message,
            ErrorCode = errorCode
        };
}