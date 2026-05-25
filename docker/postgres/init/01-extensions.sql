-- Enable extensions required by QuranCompanion.
-- pg_trgm: trigram similarity / GIN indexes for fuzzy text search (Phase 1).
-- unaccent: diacritic-insensitive search for Turkish/Arabic text.
-- vector: pgvector for embedding storage (Phase 2 RAG).

CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE EXTENSION IF NOT EXISTS unaccent;
CREATE EXTENSION IF NOT EXISTS vector;

-- Schemas owned by the application user.
CREATE SCHEMA IF NOT EXISTS quran;
CREATE SCHEMA IF NOT EXISTS import;
-- search and qa schemas are reserved for later phases; created here so EF
-- migrations don't need elevated privileges to add them.
CREATE SCHEMA IF NOT EXISTS search;
CREATE SCHEMA IF NOT EXISTS qa;
