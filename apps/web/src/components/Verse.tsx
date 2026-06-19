import type { VerseSummary } from "../api/types";

interface Props {
  verse: VerseSummary;
}

export function Verse({ verse }: Props) {
  const meal = verse.translations[0]?.text;

  return (
    <article
      id={`v${verse.verseNumber}`}
      className="py-10 first:pt-0 border-b border-border last:border-b-0 scroll-mt-32"
    >
      <header className="flex items-baseline justify-between gap-6 mb-6">
        <span className="font-serif text-3xl text-accent tabular-nums leading-none">
          {verse.verseNumber}
        </span>
        <span className="font-mono text-[10px] uppercase tracking-[0.22em] text-text-muted">
          Cüz {verse.juzNumber} · Sayfa {verse.pageNumber}
        </span>
      </header>

      <p
        lang="ar"
        dir="rtl"
        className="font-arabic text-[28px] md:text-[32px] leading-[2.2] text-text"
      >
        {verse.arabicText}
      </p>

      {meal && (
        <p className="mt-8 font-reading text-[18px] md:text-[19px] leading-[1.75] text-text-muted">
          {meal}
        </p>
      )}
    </article>
  );
}
