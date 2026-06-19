import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { getSurahDetail, getSurahVerses } from "../api/client";
import type { SurahDetail, VerseSummary } from "../api/types";
import { Eyebrow } from "../components/Eyebrow";
import { SectionRule } from "../components/SectionRule";
import { Verse } from "../components/Verse";

const PLACE_LABEL: Record<SurahDetail["revelationPlace"], string> = {
  Meccan: "Mekkî",
  Medinan: "Medenî",
  Unknown: "—",
};

export function SurahDetailPage() {
  const { number } = useParams<{ number: string }>();
  const surahNumber = number ? parseInt(number, 10) : NaN;

  const [surah, setSurah] = useState<SurahDetail | null>(null);
  const [verses, setVerses] = useState<VerseSummary[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (Number.isNaN(surahNumber)) {
      setError("Geçersiz sure numarası.");
      return;
    }

    const ctrl = new AbortController();
    setSurah(null);
    setVerses(null);
    setError(null);

    Promise.all([
      getSurahDetail(surahNumber, ctrl.signal),
      getSurahVerses(surahNumber, undefined, 1, 300, ctrl.signal),
    ])
      .then(([s, v]) => {
        setSurah(s);
        setVerses(v.items);
      })
      .catch((err) => {
        if (err.name !== "AbortError") {
          setError(err.message ?? "Bilinmeyen hata");
        }
      });

    return () => ctrl.abort();
  }, [surahNumber]);

  if (error) {
    return (
      <div className="mx-auto max-w-2xl px-6 py-32 text-center">
        <Eyebrow className="mb-4">Hata</Eyebrow>
        <p className="font-reading text-lg text-text-muted">{error}</p>
        <Link
          to="/"
          className="mt-8 inline-block font-mono text-[11px] uppercase tracking-[0.22em] text-accent hover:underline"
        >
          ← Sure listesine dön
        </Link>
      </div>
    );
  }

  if (!surah || !verses) {
    return (
      <div className="my-32 text-center font-mono text-xs uppercase tracking-[0.22em] text-text-muted">
        Yükleniyor
      </div>
    );
  }

  return (
    <article className="mx-auto max-w-3xl px-6">
      <div className="pt-12">
        <Link
          to="/"
          className="font-mono text-[11px] uppercase tracking-[0.22em] text-text-muted hover:text-text transition-colors duration-200 ease-[var(--ease-skill)]"
        >
          ← Sureler
        </Link>
      </div>

      <header className="pt-16 pb-20 md:pt-24 md:pb-24 text-center">
        <Eyebrow className="mb-6">
          Sure {surah.number} · {PLACE_LABEL[surah.revelationPlace]} · {surah.verseCount} ayet
        </Eyebrow>
        <h1 className="font-serif text-6xl md:text-7xl leading-[1.05] tracking-tight optical-display">
          {surah.nameTurkish}
        </h1>
        <p
          lang="ar"
          dir="rtl"
          className="font-arabic text-5xl md:text-6xl text-accent mt-6"
        >
          {surah.nameArabic}
        </p>
        <p className="mt-3 font-mono text-xs uppercase tracking-[0.22em] text-text-muted">
          {surah.nameTransliteration}
        </p>
        <div className="mt-12">
          <SectionRule />
        </div>
      </header>

      <div className="pb-32">
        {verses.map((v) => (
          <Verse key={v.globalVerseNumber} verse={v} />
        ))}
      </div>
    </article>
  );
}
