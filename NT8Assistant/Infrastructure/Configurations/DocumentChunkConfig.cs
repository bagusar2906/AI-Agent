using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NT8Assistant.Domain;

namespace NT8Assistant.Infrastructure.Configurations;

public class DocumentChunkConfig : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("DocumentChunks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.EmbeddingJson)
            .HasColumnType("TEXT");
    }
}