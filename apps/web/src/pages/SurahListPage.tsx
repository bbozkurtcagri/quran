import { useEffect, useState } from "react";
import { getSurahs } from "../api/client";
import type { SurahListItem } from "../api/types";
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

  if (error) {
    return <div className="state state--error">Hata: {error}</div>;
  }

  if (!surahs) {
    return <div className="state">Sureler yükleniyor…</div>;
  }

  return (
    <div className="page">
      <h1 className="page__title">Sureler</h1>
      <p className="page__lead">
        Kur'an-ı Kerim · 114 sure · {surahs.reduce((sum, s) => sum + s.verseCount, 0)} ayet
      </p>
      <div className="surah-grid">
        {surahs.map((s) => (
          <SurahCard key={s.number} surah={s} />
        ))}
      </div>
    </div>
  );
}
