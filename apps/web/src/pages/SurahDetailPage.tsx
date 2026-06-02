import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { getSurahDetail, getSurahVerses } from "../api/client";
import type { SurahDetail, VerseSummary } from "../api/types";
import { VerseCard } from "../components/VerseCard";

const PLACE_LABELS: Record<SurahDetail["revelationPlace"], string> = {
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
      <div className="page">
        <div className="state state--error">Hata: {error}</div>
        <Link to="/" className="link">
          ← Sure listesine dön
        </Link>
      </div>
    );
  }

  if (!surah || !verses) {
    return <div className="state">Yükleniyor…</div>;
  }

  return (
    <div className="page">
      <Link to="/" className="link link--back">
        ← Sure listesi
      </Link>
      <header className="surah-header">
        <div className="surah-header__number">{surah.number}</div>
        <div>
          <h1 className="surah-header__name">{surah.nameTurkish}</h1>
          <div className="surah-header__arabic" lang="ar" dir="rtl">
            {surah.nameArabic}
          </div>
          <div className="surah-header__meta">
            {PLACE_LABELS[surah.revelationPlace]} · {surah.verseCount} ayet
          </div>
        </div>
      </header>
      <div className="verses">
        {verses.map((v) => (
          <VerseCard key={v.globalVerseNumber} verse={v} />
        ))}
      </div>
    </div>
  );
}
