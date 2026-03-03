using Microsoft.EntityFrameworkCore;
using NT8Assistant.Domain;
using NT8Assistant.Infrastructure;

namespace NT8Assistant.Services;

public class ChatService
{
    private readonly AppDbContext _db;

    public ChatService(AppDbContext db)
    {
        _db = db;
    }

    public async Task SaveMessageAsync(string sessionId, string role, string content)
    {
        _db.ChatMessages.Add(new ChatMessage
        {
            SessionId = sessionId,
            Role = role,
            Content = content
        });

        await _db.SaveChangesAsync();
    }

    public async Task<List<ChatMessage>> GetHistoryAsync(string sessionId)
    {
        return await _db.ChatMessages
            .Where(x => x.SessionId == sessionId)
            .OrderBy(x => x.Id)
            .ToListAsync();
    }
}