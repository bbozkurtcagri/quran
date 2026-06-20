import { useEffect, useMemo, useState } from "react";
import { getSurahs } from "../api/client";
import type { SurahListItem } from "../api/types";
import { ContinueReading } from "../components/ContinueReading";
import { Eyebrow } from "../components/Eyebrow";
import { SectionRule } from "../components/SectionRule";
import { SurahCard } from "../components/SurahCard";
import { useLastRead } from "../hooks/useLastRead";

/** Turkish-aware light normalizer for client-side filtering. */
function normalize(input: string): string {
  if (!input) return "";
  return input
    .toLocaleLowerCase("tr")
    .normalize("NFKD")
    .replace(/\p{Diacritic}/gu, "")
    .replace(/ı/g, "i")
    .replace(/İ/g, "i")
    .replace(/[^\p{Letter}\p{Number}]+/gu, " ")
    .trim();
}

export function SurahListPage() {
  const [surahs, setSurahs] = useState<SurahListItem[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState("");

  const { lastRead } = useLastRead();

  useEffect(() => {
    const ctrl = new AbortController();
    getSurahs(ctrl.signal)
      .then(setSurahs)
      .catch((err) => {
        if (err.name !== "AbortError") {
          setError(err.message ?? "Bilinmeyen hata");
        }
      });
    return () => ctrl.abort();
  }, []);

  const filtered = useMemo(() => {
    if (!surahs) return null;
    const q = normalize(filter);
    if (!q) return surahs;

    return surahs.filter((s) => {
      const haystacks = [
        normalize(s.nameTurkish),
        normalize(s.nameTransliteration),
        s.nameArabic, // arabic literal — match raw, no normalization
        String(s.number),
        String(s.number).padStart(2, "0"),
      ];
      return haystacks.some((h) => h.includes(q));
    });
  }, [surahs, filter]);

  return (
    <div className="mx-auto max-w-5xl px-6">
      <header className="pt-20 pb-12 md:pt-28 md:pb-16 text-center">
        <Eyebrow className="mb-6">Kur'an-ı Kerim · 114 sure · 6236 ayet</Eyebrow>
        <h1 className="font-serif text-6xl md:text-8xl leading-[1.02] tracking-tight optical-display">
          Sureler
        </h1>
        <div className="mt-10">
          <SectionRule />
        </div>
      </header>

      {lastRead && (
        <div className="mx-auto max-w-md mb-10">
          <ContinueReading lastRead={lastRead} />
        </div>
      )}

      <div className="mx-auto max-w-md mb-10">
        <label htmlFor="surah-filter" className="sr-only">
          Sure ara
        </label>
        <div className="relative">
          <span
            aria-hidden
            className="pointer-events-none absolute inset-y-0 left-0 grid place-items-center pl-1 text-text-muted"
          >
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
              <circle cx="11" cy="11" r="7" />
              <path d="m20 20-3.5-3.5" />
            </svg>
          </span>
          <input
            id="surah-filter"
            type="search"
            value={filter}
            onChange={(e) => setFilter(e.target.value)}
            placeholder="Sure ara — adı veya numarası"
            spellCheck={false}
            className="w-full bg-transparent font-reading text-base md:text-lg pl-8 pr-3 py-3 border-b border-border focus:border-accent outline-none transition-colors duration-300 ease-[var(--ease-skill)] placeholder:text-text-muted/50"
          />
        </div>
      </div>

      {error && (
        <div className="my-12 mx-auto max-w-md text-center font-reading text-text-muted">
          {error}
        </div>
      )}

      {!error && !surahs && (
        <div className="my-24 text-center font-mono text-xs uppercase tracking-[0.22em] text-text-muted">
          Yükleniyor
        </div>
      )}

      {filtered && filtered.length === 0 && (
        <div className="my-16 text-center font-reading text-text-muted">
          <span className="block mb-2">"{filter}" için sure bulunamadı.</span>
          <button
            type="button"
            onClick={() => setFilter("")}
            className="font-mono text-[11px] uppercase tracking-[0.22em] text-accent hover:underline"
          >
            Aramayı temizle
          </button>
        </div>
      )}

      {filtered && filtered.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 md:gap-x-12 pb-24">
          {filtered.map((s) => (
            <SurahCard key={s.number} surah={s} />
          ))}
        </div>
      )}
    </div>
  );
}
