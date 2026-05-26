# QuranCompanion

Backend for a Quran reading, translation and search application — and the
foundation for a citation-only question/answer layer in a later phase.

## Phase plan

**Phase 1 — reading & search (current).** Sure list, sure detail, verse
listing, multi-source translation support, full-text search across Arabic and
Turkish meals. No authentication.

**Phase 2 — citation-only Q&A (future).** Natural-language questions answered
strictly from retrieved verses, translations and (later) tafsir. The system
must surface citations and explicitly flag topics that the Quran does not
address directly. Implementation is gated behind product decisions; the schema
reserves namespaces (`qa`, `search`) but no tables exist yet.

## Architecture

Four-project layered solution with CQRS via MediatR.

```
src/
  QuranCompanion.Api             — ASP.NET Core Web API, controllers, middleware, seed CLI
  QuranCompanion.Application     — Queries/handlers, validators, DTOs, abstractions
  QuranCompanion.Domain          — Entities, enums, base types
  QuranCompanion.Infrastructure  — EF Core, DbContext, migrations, persistence

tests/
  QuranCompanion.Application.Tests
```

Dependencies flow inward only: `Api → Infrastructure → Application → Domain`.
The Application layer talks to the database through `IApplicationDbContext`,
which exposes `DbSet<>` so query handlers can compose efficient EF queries
without depending on the concrete `ApplicationDbContext`.

### Database

PostgreSQL 17 with extensions:

- `pg_trgm` — trigram GIN index on `verse_translations.normalized_text` for
  case- and accent-insensitive search.
- `unaccent` — diacritic stripping for SQL-side fallback.
- `vector` (pgvector) — reserved for Phase 2 embeddings; no tables yet.

Schemas:

| Schema    | Purpose                                         |
| --------- | ----------------------------------------------- |
| `quran`   | `surahs`, `verses`, `translation_sources`, `verse_translations` |
| `import`  | `import_histories` — idempotency log            |
| `search`  | Reserved for Phase 2 read models (empty)        |
| `qa`      | Reserved for Phase 2 Q&A tables (empty)         |

## Local development

### 1. Start PostgreSQL

```bash
docker compose up -d
```

This runs `pgvector/pgvector:pg17` on port 5432, creates the database
`qurancompanion`, and runs `docker/postgres/init/01-extensions.sql` which
installs all required extensions and schemas.

### 2. Apply migrations and seed

The seed CLI applies migrations and imports JSON files from `seed-data/`:

```bash
dotnet run --project src/QuranCompanion.Api -- seed
```

The seed is **idempotent** — each file is hashed (SHA256) and recorded in
`import.import_histories`. Re-running with unchanged content is a no-op.

No data is committed to the repository. See [`seed-data/README.md`](seed-data/README.md)
for the expected JSON layouts and licensing notes for upstream Quran/meal
sources.

### 3. Run the API

```bash
dotnet run --project src/QuranCompanion.Api
```

The default profile listens on `http://localhost:5000`. In Development, the
OpenAPI document is served at `/openapi/v1.json` and a [Scalar](https://github.com/scalar/scalar)
reference UI is mounted at `/scalar/v1`.

### 4. Run tests

```bash
dotnet test
```

## API endpoints (Phase 1)

All responses use the envelope:

```json
{ "data": { ... }, "success": true, "message": null, "errors": [] }
```

| Method | Path                                                | Notes                                |
| ------ | --------------------------------------------------- | ------------------------------------ |
| GET    | `/api/v1/surahs`                                    | All 114 surahs.                      |
| GET    | `/api/v1/surahs/{number}`                           | Surah detail.                        |
| GET    | `/api/v1/surahs/{number}/verses`                    | Paged verses with translations.      |
| GET    | `/api/v1/verses/{surah}/{verse}`                    | Single verse + translations.         |
| GET    | `/api/v1/verses/by-global-number/{n}`               | Mushaf order (1–6236).               |
| GET    | `/api/v1/translation-sources`                       | Registered translations.             |
| GET    | `/api/v1/search?query=…&translationSourceCode=…`    | Full-text search across meals.       |
| GET    | `/health/live`                                      | Liveness.                            |
| GET    | `/health/ready`                                     | Readiness incl. DB.                  |

A fixed-window rate limiter (100 req/min per IP) is applied globally.

## Configuration

Connection string lives in `appsettings.json` under `ConnectionStrings:Postgres`.
For local development, override via user secrets or environment variable
`ConnectionStrings__Postgres`. The EF design-time factory reads
`QURANCOMPANION_POSTGRES` so `dotnet ef migrations` works without an active
configuration source.

## EF Core

Generate a new migration:

```bash
dotnet ef migrations add <Name> \
  --project src/QuranCompanion.Infrastructure \
  --startup-project src/QuranCompanion.Infrastructure \
  --output-dir Persistence/Migrations
```

Apply migrations at runtime via the seed CLI (`dotnet run … -- seed`) — it
calls `Database.MigrateAsync()` before importing.

## Tests

`dotnet test` runs Application tests. EF in-memory provider is wired up for
future query handler tests; the text normalizer is covered today.

## License

See [`seed-data/README.md`](seed-data/README.md) for notes on Quran/meal data
licensing. The application code itself is unlicensed for now; pick a license
before publishing.
