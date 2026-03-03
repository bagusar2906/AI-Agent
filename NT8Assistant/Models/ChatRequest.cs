namespace NT8Assistant.Models;

public class ChatRequest
{
    public List<ChatMessage> Messages { get; set; } = new();
}