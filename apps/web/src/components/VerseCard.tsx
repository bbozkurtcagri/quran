import type { VerseSummary } from "../api/types";

interface Props {
  verse: VerseSummary;
}

export function VerseCard({ verse }: Props) {
  const meal = verse.translations[0]?.text;

  return (
    <article className="verse">
      <header className="verse__header">
        <span className="verse__number">
          {verse.surahNumber}:{verse.verseNumber}
        </span>
        <span className="verse__meta">
          Cüz {verse.juzNumber} · Sayfa {verse.pageNumber}
        </span>
      </header>
      <p className="verse__arabic" lang="ar" dir="rtl">
        {verse.arabicText}
      </p>
      {meal && <p className="verse__meal">{meal}</p>}
    </article>
  );
}
