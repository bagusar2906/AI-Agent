namespace NT8Assistant.Models;

public class AgentMessage
{
    public string Role { get; set; } = "assistant";
    public string Type { get; set; } = "chat"; // chat | thinking | tool | validation | error
    public object? Content { get; set; }
}