using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuranCompanion.Domain.Quran;
using QuranCompanion.Infrastructure.Persistence.Constants;

namespace QuranCompanion.Infrastructure.Persistence.Configurations;

internal sealed class SurahConfiguration : IEntityTypeConfiguration<Surah>
{
    public void Configure(EntityTypeBuilder<Surah> builder)
    {
        builder.ToTable("surahs", Schemas.Quran);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Number)
            .IsRequired();

        builder.Property(x => x.NameArabic)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.NameTurkish)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.NameTransliteration)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.VerseCount).IsRequired();

        builder.Property(x => x.RevelationPlace)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.DisplayOrder).IsRequired();

        builder.Property(x => x.CreatedOnUtc).IsRequired();

        builder.HasIndex(x => x.Number)
            .IsUnique()
            .HasDatabaseName("ux_surahs_number");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
