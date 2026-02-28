using System.ComponentModel.DataAnnotations;

namespace HybridAgent.Domain;

public class ChatMessage
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = default!;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = default!; // user / assistant

    [Required]
    public string Content { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}