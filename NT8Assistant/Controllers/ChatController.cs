using Microsoft.AspNetCore.Mvc;
using NT8Assistant.Models;
using NT8Assistant.Services;

namespace NT8Assistant.Controllers;

[ApiController]
[Route("chat")]
public class ChatController(IAgent agent, ChatService chatService, ILogger<ChatController> logger) : ControllerBase
{
    [HttpGet("stream")]
    public async Task Stream(string message)
    {
        Response.ContentType = "text/plain";

        logger.LogInformation("User message received: {UserMessage}",message);
        await foreach (var chunk in agent.RunAsync(message))
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
            logger.LogInformation("User message received: {UserMessage}", userMessage);

            await foreach (var chunk in agent.RunAsync(userMessage))
            {
                await Response.WriteAsync($"data: {chunk}\n\n");
                await Response.Body.FlushAsync();
            }

            await Response.WriteAsync("data: [DONE]\n\n");

        await chatService.SaveMessageAsync(sessionId, "user", userMessage);
    }
}