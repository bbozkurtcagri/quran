import { Link } from "react-router-dom";
import type { SurahListItem } from "../api/types";
import { Eyebrow } from "./Eyebrow";

interface Props {
  surah: SurahListItem;
}

const PLACE_LABEL: Record<SurahListItem["revelationPlace"], string> = {
  Meccan: "Mekkî",
  Medinan: "Medenî",
  Unknown: "—",
};

export function SurahCard({ surah }: Props) {
  return (
    <Link
      to={`/surahs/${surah.number}`}
      aria-label={`${surah.nameTurkish} suresine git`}
      className="
        group relative grid grid-cols-[auto_1fr_auto_auto] items-center gap-6
        px-5 py-6 border-b border-border last:border-b-0
        transition-colors duration-200 ease-[var(--ease-skill)]
        hover:bg-accent-soft/60 focus-visible:bg-accent-soft/60 focus-visible:outline-none
        before:absolute before:left-0 before:top-3 before:bottom-3 before:w-[2px]
        before:bg-accent before:rounded-full
        before:opacity-0 before:transition-opacity before:duration-200
        hover:before:opacity-100 focus-visible:before:opacity-100
      "
    >
      <span className="font-mono text-xs text-text-muted tabular-nums w-8 select-none transition-colors duration-200 group-hover:text-accent group-focus-visible:text-accent">
        {String(surah.number).padStart(2, "0")}
      </span>

      <div className="min-w-0">
        <h2 className="font-serif text-2xl md:text-[28px] leading-[1.1] tracking-tight optical-display">
          {surah.nameTurkish}
        </h2>
        <Eyebrow className="mt-2">
          {PLACE_LABEL[surah.revelationPlace]} · {surah.verseCount} ayet · {surah.nameTransliteration}
        </Eyebrow>
      </div>

      <span
        lang="ar"
        dir="rtl"
        className="font-arabic text-2xl md:text-[28px] text-text leading-none transition-colors duration-200 ease-[var(--ease-skill)] group-hover:text-accent group-focus-visible:text-accent"
      >
        {surah.nameArabic}
      </span>

      <span
        aria-hidden
        className="font-mono text-sm text-accent opacity-0 -translate-x-2 transition-all duration-200 ease-[var(--ease-skill)] group-hover:opacity-100 group-hover:translate-x-0 group-focus-visible:opacity-100 group-focus-visible:translate-x-0"
      >
        →
      </span>
    </Link>
  );
}
