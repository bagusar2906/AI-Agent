using Microsoft.EntityFrameworkCore;
using NT8Assistant.Domain;

namespace NT8Assistant.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}