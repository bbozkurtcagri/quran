using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuranCompanion.Domain.Quran;
using QuranCompanion.Infrastructure.Persistence.Constants;

namespace QuranCompanion.Infrastructure.Persistence.Configurations;

internal sealed class VerseConfiguration : IEntityTypeConfiguration<Verse>
{
    public void Configure(EntityTypeBuilder<Verse> builder)
    {
        builder.ToTable("verses", Schemas.Quran);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SurahNumber).IsRequired();
        builder.Property(x => x.VerseNumber).IsRequired();
        builder.Property(x => x.GlobalVerseNumber).IsRequired();
        builder.Property(x => x.JuzNumber).IsRequired();
        builder.Property(x => x.PageNumber).IsRequired();

        builder.Property(x => x.ArabicText)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.ArabicTextClean)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.CreatedOnUtc).IsRequired();

        builder.HasOne(x => x.Surah)
            .WithMany(x => x.Verses)
            .HasForeignKey(x => x.SurahId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.SurahNumber, x.VerseNumber })
            .IsUnique()
            .HasDatabaseName("ux_verses_surah_verse");

        builder.HasIndex(x => x.GlobalVerseNumber)
            .IsUnique()
            .HasDatabaseName("ux_verses_global_verse_number");

        builder.HasIndex(x => x.SurahId)
            .HasDatabaseName("ix_verses_surah_id");

        builder.HasIndex(x => x.JuzNumber)
            .HasDatabaseName("ix_verses_juz_number");

        builder.HasIndex(x => x.PageNumber)
            .HasDatabaseName("ix_verses_page_number");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
