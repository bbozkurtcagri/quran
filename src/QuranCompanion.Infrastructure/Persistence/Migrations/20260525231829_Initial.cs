using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuranCompanion.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "import");

            migrationBuilder.EnsureSchema(
                name: "quran");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "import_histories",
                schema: "import",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    import_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    file_name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    content_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    inserted_count = table.Column<int>(type: "integer", nullable: false),
                    updated_count = table.Column<int>(type: "integer", nullable: false),
                    failed_count = table.Column<int>(type: "integer", nullable: false),
                    started_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_import_histories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "surahs",
                schema: "quran",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    number = table.Column<int>(type: "integer", nullable: false),
                    name_arabic = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name_turkish = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name_transliteration = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    verse_count = table.Column<int>(type: "integer", nullable: false),
                    revelation_place = table.Column<int>(type: "integer", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_surahs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "translation_sources",
                schema: "quran",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    language_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    author = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    license_info = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    source_url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_translation_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "verses",
                schema: "quran",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    surah_id = table.Column<long>(type: "bigint", nullable: false),
                    surah_number = table.Column<int>(type: "integer", nullable: false),
                    verse_number = table.Column<int>(type: "integer", nullable: false),
                    global_verse_number = table.Column<int>(type: "integer", nullable: false),
                    juz_number = table.Column<int>(type: "integer", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: false),
                    arabic_text = table.Column<string>(type: "text", nullable: false),
                    arabic_text_clean = table.Column<string>(type: "text", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verses", x => x.id);
                    table.ForeignKey(
                        name: "fk_verses_surahs_surah_id",
                        column: x => x.surah_id,
                        principalSchema: "quran",
                        principalTable: "surahs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "verse_translations",
                schema: "quran",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    verse_id = table.Column<long>(type: "bigint", nullable: false),
                    translation_source_id = table.Column<long>(type: "bigint", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    normalized_text = table.Column<string>(type: "text", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verse_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_verse_translations_translation_sources_translation_source_id",
                        column: x => x.translation_source_id,
                        principalSchema: "quran",
                        principalTable: "translation_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_verse_translations_verses_verse_id",
                        column: x => x.verse_id,
                        principalSchema: "quran",
                        principalTable: "verses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_import_histories_source_type_hash",
                schema: "import",
                table: "import_histories",
                columns: new[] { "source_code", "import_type", "content_hash" });

            migrationBuilder.CreateIndex(
                name: "ux_surahs_number",
                schema: "quran",
                table: "surahs",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_translation_sources_code",
                schema: "quran",
                table: "translation_sources",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_verse_translations_normalized_text_trgm",
                schema: "quran",
                table: "verse_translations",
                column: "normalized_text")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_verse_translations_source",
                schema: "quran",
                table: "verse_translations",
                column: "translation_source_id");

            migrationBuilder.CreateIndex(
                name: "ux_verse_translations_verse_source",
                schema: "quran",
                table: "verse_translations",
                columns: new[] { "verse_id", "translation_source_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_verses_juz_number",
                schema: "quran",
                table: "verses",
                column: "juz_number");

            migrationBuilder.CreateIndex(
                name: "ix_verses_page_number",
                schema: "quran",
                table: "verses",
                column: "page_number");

            migrationBuilder.CreateIndex(
                name: "ix_verses_surah_id",
                schema: "quran",
                table: "verses",
                column: "surah_id");

            migrationBuilder.CreateIndex(
                name: "ux_verses_global_verse_number",
                schema: "quran",
                table: "verses",
                column: "global_verse_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_verses_surah_verse",
                schema: "quran",
                table: "verses",
                columns: new[] { "surah_number", "verse_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "import_histories",
                schema: "import");

            migrationBuilder.DropTable(
                name: "verse_translations",
                schema: "quran");

            migrationBuilder.DropTable(
                name: "translation_sources",
                schema: "quran");

            migrationBuilder.DropTable(
                name: "verses",
                schema: "quran");

            migrationBuilder.DropTable(
                name: "surahs",
                schema: "quran");
        }
    }
}
