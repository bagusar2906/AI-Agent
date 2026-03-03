using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NT8Assistant.Domain;

namespace NT8Assistant.Infrastructure.Configurations;

public class ChatMessageConfig : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.SessionId);

        builder.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}