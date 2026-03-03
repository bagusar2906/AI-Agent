using Microsoft.EntityFrameworkCore;
using NT8Assistant.Domain;
using NT8Assistant.Infrastructure;

namespace NT8Assistant.Services;

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