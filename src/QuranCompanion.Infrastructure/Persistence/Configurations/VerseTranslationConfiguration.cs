using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuranCompanion.Domain.Quran;
using QuranCompanion.Infrastructure.Persistence.Constants;

namespace QuranCompanion.Infrastructure.Persistence.Configurations;

internal sealed class VerseTranslationConfiguration : IEntityTypeConfiguration<VerseTranslation>
{
    public void Configure(EntityTypeBuilder<VerseTranslation> builder)
    {
        builder.ToTable("verse_translations", Schemas.Quran);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.NormalizedText)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.CreatedOnUtc).IsRequired();

        builder.HasOne(x => x.Verse)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.VerseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TranslationSource)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.TranslationSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.VerseId, x.TranslationSourceId })
            .IsUnique()
            .HasDatabaseName("ux_verse_translations_verse_source");

        builder.HasIndex(x => x.TranslationSourceId)
            .HasDatabaseName("ix_verse_translations_source");

        // pg_trgm GIN index on normalized_text for fuzzy/contains search.
        builder.HasIndex(x => x.NormalizedText)
            .HasDatabaseName("ix_verse_translations_normalized_text_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
