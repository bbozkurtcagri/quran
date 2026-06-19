import { useEffect, useState } from "react";
import { getSurahs } from "../api/client";
import type { SurahListItem } from "../api/types";
import { Eyebrow } from "../components/Eyebrow";
import { SectionRule } from "../components/SectionRule";
import { SurahCard } from "../components/SurahCard";

export function SurahListPage() {
  const [surahs, setSurahs] = useState<SurahListItem[] | null>(null);
  const [error, setError] = useState<string | null>(null);

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

  return (
    <div className="mx-auto max-w-5xl px-6">
      <header className="pt-20 pb-16 md:pt-28 md:pb-20 text-center">
        <Eyebrow className="mb-6">Kur'an-ı Kerim · 114 sure · 6236 ayet</Eyebrow>
        <h1 className="font-serif text-6xl md:text-8xl leading-[1.02] tracking-tight optical-display">
          Sureler
        </h1>
        <div className="mt-10">
          <SectionRule />
        </div>
      </header>

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

      {surahs && (
        <div className="grid grid-cols-1 md:grid-cols-2 md:gap-x-12 pb-24">
          {surahs.map((s) => (
            <SurahCard key={s.number} surah={s} />
          ))}
        </div>
      )}
    </div>
  );
}
