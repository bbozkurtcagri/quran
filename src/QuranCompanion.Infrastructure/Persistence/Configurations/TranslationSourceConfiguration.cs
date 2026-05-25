using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuranCompanion.Domain.Quran;
using QuranCompanion.Infrastructure.Persistence.Constants;

namespace QuranCompanion.Infrastructure.Persistence.Configurations;

internal sealed class TranslationSourceConfiguration : IEntityTypeConfiguration<TranslationSource>
{
    public void Configure(EntityTypeBuilder<TranslationSource> builder)
    {
        builder.ToTable("translation_sources", Schemas.Quran);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.LanguageCode)
            .IsRequired()
            .HasMaxLength(8);

        builder.Property(x => x.Author)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Description).HasMaxLength(2048);
        builder.Property(x => x.LicenseInfo).HasMaxLength(1024);
        builder.Property(x => x.SourceUrl).HasMaxLength(1024);

        builder.Property(x => x.CreatedOnUtc).IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("ux_translation_sources_code");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
