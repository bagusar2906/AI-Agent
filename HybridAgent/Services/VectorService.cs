using HybridAgent.Domain;
using HybridAgent.Infrastructure;

namespace HybridAgent.Services;

using Microsoft.EntityFrameworkCore;

public class VectorService
{
    private readonly AppDbContext _db;

    public VectorService(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddDocumentAsync(string content)
    {
        _db.DocumentChunks.Add(new DocumentChunk
        {
            Content = content
        });

        await _db.SaveChangesAsync();
    }

    public async Task<List<string>> SearchAsync(string query)
    {
        return await _db.DocumentChunks
            .Where(d => d.Content.Contains(query))
            .Select(d => d.Content)
            .Take(3)
            .ToListAsync();
    }
}

public class SearchArgs
{
    public string Query { get; set; } = string.Empty;
}