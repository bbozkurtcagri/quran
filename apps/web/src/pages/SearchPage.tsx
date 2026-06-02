import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { searchVerses } from "../api/client";
import type { PagedResult, VerseSearchHit } from "../api/types";

export function SearchPage() {
  const [params, setParams] = useSearchParams();
  const initialQuery = params.get("q") ?? "";

  const [input, setInput] = useState(initialQuery);
  const [results, setResults] = useState<PagedResult<VerseSearchHit> | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const q = params.get("q")?.trim();
    if (!q || q.length < 2) {
      setResults(null);
      return;
    }

    const ctrl = new AbortController();
    setLoading(true);
    setError(null);

    searchVerses(q, 1, 20, undefined, ctrl.signal)
      .then(setResults)
      .catch((err) => {
        if (err.name !== "AbortError") {
          setError(err.message ?? "Bilinmeyen hata");
        }
      })
      .finally(() => setLoading(false));

    return () => ctrl.abort();
  }, [params]);

  function onSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const trimmed = input.trim();
    if (trimmed.length < 2) {
      setError("Arama metni en az 2 karakter olmalı.");
      return;
    }
    setParams({ q: trimmed });
  }

  return (
    <div className="page">
      <h1 className="page__title">Meal araması</h1>
      <p className="page__lead">Elmalılı meali içinde arama yapar.</p>
      <form className="search-form" onSubmit={onSubmit}>
        <input
          type="text"
          className="search-input"
          placeholder="örn. sabır, namaz, adalet"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          autoFocus
        />
        <button type="submit" className="search-button">
          Ara
        </button>
      </form>

      {error && <div className="state state--error">Hata: {error}</div>}
      {loading && <div className="state">Aranıyor…</div>}

      {results && !loading && (
        <div className="search-results">
          <div className="search-results__summary">
            <strong>{results.totalCount}</strong> sonuç bulundu
            {results.totalCount > results.items.length && (
              <span> · ilk {results.items.length} gösteriliyor</span>
            )}
          </div>
          {results.items.length === 0 && (
            <div className="state">Sonuç yok.</div>
          )}
          {results.items.map((hit) => (
            <Link
              key={hit.globalVerseNumber}
              to={`/surahs/${hit.surahNumber}#v${hit.verseNumber}`}
              className="search-hit"
            >
              <div className="search-hit__location">
                {hit.surahNameTurkish} · {hit.surahNumber}:{hit.verseNumber}
              </div>
              <div className="search-hit__text">{hit.translationText}</div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
