using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuranCompanion.Domain.Import;
using QuranCompanion.Infrastructure.Persistence.Constants;

namespace QuranCompanion.Infrastructure.Persistence.Configurations;

internal sealed class ImportHistoryConfiguration : IEntityTypeConfiguration<ImportHistory>
{
    public void Configure(EntityTypeBuilder<ImportHistory> builder)
    {
        builder.ToTable("import_histories", Schemas.Import);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceCode)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.ImportType)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.ContentHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasColumnType("text");

        builder.Property(x => x.CreatedOnUtc).IsRequired();

        builder.HasIndex(x => new { x.SourceCode, x.ImportType, x.ContentHash })
            .HasDatabaseName("ix_import_histories_source_type_hash");
    }
}
