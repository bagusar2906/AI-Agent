using System.ComponentModel.DataAnnotations;

namespace NT8Assistant.Domain;

public class DocumentChunk
{
    public int Id { get; set; }

    [Required]
    public string Content { get; set; } = default!;

    // Future upgrade: embedding column
    public string? EmbeddingJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}