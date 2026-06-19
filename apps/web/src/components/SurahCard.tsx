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
      className="group relative grid grid-cols-[auto_1fr_auto] items-center gap-6 px-4 py-6 border-b border-border last:border-b-0 transition-colors duration-200 ease-[var(--ease-skill)] hover:bg-accent-soft/50"
    >
      <span className="font-mono text-xs text-text-muted tabular-nums w-8">
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
        className="font-arabic text-2xl md:text-[28px] text-text leading-none transition-colors duration-200 ease-[var(--ease-skill)] group-hover:text-accent"
      >
        {surah.nameArabic}
      </span>
    </Link>
  );
}
