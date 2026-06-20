import { useEffect, useMemo, useRef, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { searchVerses, type SearchMode } from "../api/client";
import type { PagedResult, VerseSearchHit } from "../api/types";
import { Eyebrow } from "../components/Eyebrow";
import { SectionRule } from "../components/SectionRule";

const DEBOUNCE_MS = 300;
const MIN_QUERY = 2;
const MODES: { value: SearchMode; label: string }[] = [
  { value: "semantic", label: "Anlamsal" },
  { value: "keyword", label: "Sözcük" },
];

function isMode(v: string | null): v is SearchMode {
  return v === "semantic" || v === "keyword";
}

export function SearchPage() {
  const [params, setParams] = useSearchParams();
  const initialQuery = params.get("q") ?? "";
  const initialMode: SearchMode = isMode(params.get("mode")) ? (params.get("mode") as SearchMode) : "semantic";

  const [input, setInput] = useState(initialQuery);
  const [mode, setMode] = useState<SearchMode>(initialMode);
  const [results, setResults] = useState<PagedResult<VerseSearchHit> | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Push input + mode into the URL with a quiet debounce on the text.
  const debounceRef = useRef<number | undefined>(undefined);
  useEffect(() => {
    window.clearTimeout(debounceRef.current);
    debounceRef.current = window.setTimeout(() => {
      const trimmed = input.trim();
      const next = new URLSearchParams();
      if (trimmed.length >= MIN_QUERY) {
        next.set("q", trimmed);
      }
      if (mode !== "semantic") {
        next.set("mode", mode);
      }
      const currentQs = params.toString();
      const nextQs = next.toString();
      if (currentQs !== nextQs) {
        setParams(next, { replace: true });
      }
    }, DEBOUNCE_MS);

    return () => window.clearTimeout(debounceRef.current);
  }, [input, mode, params, setParams]);

  // Fetch when URL params change.
  useEffect(() => {
    const q = params.get("q")?.trim();
    const m: SearchMode = isMode(params.get("mode")) ? (params.get("mode") as SearchMode) : "semantic";

    if (!q || q.length < MIN_QUERY) {
      setResults(null);
      setError(null);
      return;
    }

    const ctrl = new AbortController();
    setLoading(true);
    setError(null);

    searchVerses(q, 1, 20, undefined, m, ctrl.signal)
      .then(setResults)
      .catch((err) => {
        if (err.name !== "AbortError") {
          setError(err.message ?? "Bilinmeyen hata");
        }
      })
      .finally(() => setLoading(false));

    return () => ctrl.abort();
  }, [params]);

  const queryNonce = params.get("q") ?? "";
  const activeMode: SearchMode = isMode(params.get("mode")) ? (params.get("mode") as SearchMode) : "semantic";

  const totalLabel = useMemo(() => {
    if (!results) return null;
    // In semantic mode every embedded verse is technically scored, so the
    // raw totalCount is meaningless — show the displayed slice instead.
    if (activeMode === "semantic") {
      return `İlk ${results.items.length} sonuç`;
    }
    if (results.totalCount === 0) return "Sonuç yok";
    if (results.totalCount > results.items.length) {
      return `${results.totalCount} sonuç · ilk ${results.items.length} gösteriliyor`;
    }
    return `${results.totalCount} sonuç`;
  }, [results, activeMode]);

  return (
    <div className="mx-auto max-w-3xl px-6">
      <header className="pt-20 pb-16 md:pt-28 md:pb-20 text-center">
        <Eyebrow className="mb-6">Elmalılı meali içinde arama</Eyebrow>
        <h1 className="font-serif text-6xl md:text-7xl leading-[1.05] tracking-tight optical-display">
          Arama
        </h1>
        <div className="mt-10">
          <SectionRule />
        </div>
      </header>

      <div className="pb-4">
        <label htmlFor="search-input" className="sr-only">
          Arama metni
        </label>
        <input
          id="search-input"
          type="text"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder={
            mode === "semantic"
              ? "Tahammül, takva, helal-haram, israf…"
              : "örn. sabır · namaz · adalet"
          }
          autoFocus
          spellCheck={false}
          className="w-full bg-transparent font-reading text-2xl md:text-3xl py-4 border-b border-border focus:border-accent outline-none transition-colors duration-300 ease-[var(--ease-skill)] placeholder:text-text-muted/60"
        />
      </div>

      <div className="flex items-center justify-between pb-8">
        <ModeSegmented value={mode} onChange={setMode} />
        <Eyebrow>{mode === "semantic" ? "Kavramsal" : "Birebir eşleşme"}</Eyebrow>
      </div>

      <div className="min-h-[40vh] pb-32">
        {error && (
          <p className="my-12 text-center font-reading text-text-muted">{error}</p>
        )}

        {!error && loading && (
          <p className="my-12 text-center font-mono text-[11px] uppercase tracking-[0.22em] text-text-muted">
            Aranıyor
          </p>
        )}

        {!error && results && !loading && (
          <>
            <div className="flex items-center justify-between py-4 border-b border-border">
              <Eyebrow>{totalLabel}</Eyebrow>
              <Eyebrow>Elmalılı</Eyebrow>
            </div>

            {results.items.length === 0 && (
              <p className="my-16 text-center font-reading text-text-muted">
                Sonuç bulunamadı.
              </p>
            )}

            {results.items.map((hit) => (
              <SearchResult
                key={`${hit.globalVerseNumber}-${hit.translationSourceCode}`}
                hit={hit}
                highlight={activeMode === "keyword" ? queryNonce : ""}
              />
            ))}
          </>
        )}

        {!error && !loading && !results && input.trim().length === 0 && (
          <p className="my-16 text-center font-reading text-text-muted">
            Aramaya başlamak için bir kelime veya kavram yazın.
          </p>
        )}
      </div>
    </div>
  );
}

function ModeSegmented({
  value,
  onChange,
}: {
  value: SearchMode;
  onChange: (m: SearchMode) => void;
}) {
  return (
    <div
      role="group"
      aria-label="Arama modu"
      className="inline-flex rounded-full border border-border p-0.5 bg-bg-elev/60"
    >
      {MODES.map((m) => {
        const active = m.value === value;
        return (
          <button
            key={m.value}
            type="button"
            onClick={() => onChange(m.value)}
            aria-pressed={active}
            className={
              "px-4 py-1.5 rounded-full text-xs font-mono uppercase tracking-[0.18em] transition-colors duration-200 ease-[var(--ease-skill)] " +
              (active
                ? "bg-accent-soft text-accent"
                : "text-text-muted hover:text-text")
            }
          >
            {m.label}
          </button>
        );
      })}
    </div>
  );
}

function SearchResult({ hit, highlight }: { hit: VerseSearchHit; highlight: string }) {
  return (
    <Link
      to={`/surahs/${hit.surahNumber}#v${hit.verseNumber}`}
      className="block py-8 border-b border-border group transition-colors duration-200 ease-[var(--ease-skill)] hover:bg-accent-soft/50 -mx-2 px-2 rounded-sm"
    >
      <div className="flex items-baseline justify-between gap-4 mb-3">
        <Eyebrow>
          {hit.surahNameTurkish} · {hit.surahNumber}:{hit.verseNumber}
        </Eyebrow>
        <span className="font-mono text-[10px] uppercase tracking-[0.22em] text-accent opacity-0 group-hover:opacity-100 transition-opacity duration-200 ease-[var(--ease-skill)]">
          Oku ↗
        </span>
      </div>
      <p className="font-reading text-[17px] leading-[1.7] text-text">
        {renderWithHighlight(hit.translationText, highlight)}
      </p>
    </Link>
  );
}

function renderWithHighlight(text: string, query: string) {
  const trimmed = query.trim();
  if (trimmed.length < MIN_QUERY) return text;

  const lowered = text.toLocaleLowerCase("tr");
  const target = trimmed.toLocaleLowerCase("tr");
  const parts: Array<{ text: string; match: boolean }> = [];
  let i = 0;
  while (i < text.length) {
    const idx = lowered.indexOf(target, i);
    if (idx === -1) {
      parts.push({ text: text.slice(i), match: false });
      break;
    }
    if (idx > i) {
      parts.push({ text: text.slice(i, idx), match: false });
    }
    parts.push({ text: text.slice(idx, idx + target.length), match: true });
    i = idx + target.length;
  }

  return parts.map((p, k) =>
    p.match ? (
      <mark
        key={k}
        className="bg-transparent text-accent font-[500] underline decoration-accent/40 underline-offset-[3px]"
      >
        {p.text}
      </mark>
    ) : (
      <span key={k}>{p.text}</span>
    ),
  );
}
