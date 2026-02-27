using HybridAgent.Models;
using HybridAgent.Services;
using Microsoft.AspNetCore.Mvc;

namespace HybridAgent.Controllers;

[ApiController]
[Route("v1/chat")]
public class ChatController : ControllerBase
{
    private readonly OllamaService _ollama;
    private readonly ChatService _chatService;

    public ChatController(OllamaService ollama, ChatService chatService)
    {
        _ollama = ollama;
        _chatService = chatService;
    }

    [HttpPost("completions")]
    public async Task StreamChat([FromBody] ChatRequest request)
    {
        var sessionId = "default";

        // ✅ Set headers FIRST
        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";


        var userMessage = request.Messages.Last().Content;

        await foreach (var chunk in _ollama.StreamAsync(userMessage))
        {
            await Response.WriteAsync($"data: {chunk}\n\n");
            await Response.Body.FlushAsync();
        }

        await Response.WriteAsync("data: [DONE]\n\n");

        await _chatService.SaveMessageAsync(sessionId, "user", userMessage);
    }
}