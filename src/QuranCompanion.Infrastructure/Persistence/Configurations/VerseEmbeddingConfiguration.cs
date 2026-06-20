using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuranCompanion.Domain.Search;
using QuranCompanion.Infrastructure.Persistence.Constants;

namespace QuranCompanion.Infrastructure.Persistence.Configurations;

internal sealed class VerseEmbeddingConfiguration : IEntityTypeConfiguration<VerseEmbedding>
{
    /// <summary>Dimension of intfloat/multilingual-e5-small. Fixed at the schema level.</summary>
    public const int Dimension = 384;

    public void Configure(EntityTypeBuilder<VerseEmbedding> builder)
    {
        builder.ToTable("verse_embeddings", Schemas.Search);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ModelName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.ContentHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Embedding)
            .HasColumnType($"vector({Dimension})")
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc).IsRequired();

        builder.HasOne(x => x.Verse)
            .WithMany()
            .HasForeignKey(x => x.VerseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TranslationSource)
            .WithMany()
            .HasForeignKey(x => x.TranslationSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        // One vector per (verse, source, model) — re-indexing under a new
        // model adds rows alongside the old, lets us A/B without a backfill.
        builder.HasIndex(x => new { x.VerseId, x.TranslationSourceId, x.ModelName })
            .IsUnique()
            .HasDatabaseName("ux_verse_embeddings_verse_source_model");

        // HNSW index on cosine distance — matches how Pgvector.EntityFrameworkCore
        // exposes EF.Functions.CosineDistance / the <=> operator.
        builder.HasIndex(x => x.Embedding)
            .HasDatabaseName("ix_verse_embeddings_embedding_hnsw")
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
