using HybridAgent.Models;
using HybridAgent.Services;
using Microsoft.AspNetCore.Mvc;

namespace HybridAgent.Controllers;

[ApiController]
[Route("chat")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;

    private readonly AgentService _agent;

    public ChatController(AgentService agent, ChatService chatService)
    {
        _agent = agent;
        _chatService = chatService;
    }

    [HttpGet("stream")]
    public async Task Stream(string message)
    {
        Response.ContentType = "text/plain";

        await foreach (var chunk in _agent.RunAsync(message))
        {
            await Response.WriteAsync(chunk);
            await Response.Body.FlushAsync();
        }
    }

    [HttpPost("completions")]
    public async Task StreamChat([FromBody] ChatRequest request)
    {
        var sessionId = "default";

            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            var userMessage = request.Messages.Last().Content;

            await foreach (var chunk in _agent.RunAsync(userMessage))
            {
                await Response.WriteAsync($"data: {chunk}\n\n");
                await Response.Body.FlushAsync();
            }

            await Response.WriteAsync("data: [DONE]\n\n");

        await _chatService.SaveMessageAsync(sessionId, "user", userMessage);
    }
}