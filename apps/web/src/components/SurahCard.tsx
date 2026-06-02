import { Link } from "react-router-dom";
import type { SurahListItem } from "../api/types";

interface Props {
  surah: SurahListItem;
}

const PLACE_LABELS: Record<SurahListItem["revelationPlace"], string> = {
  Meccan: "Mekkî",
  Medinan: "Medenî",
  Unknown: "—",
};

export function SurahCard({ surah }: Props) {
  return (
    <Link to={`/surahs/${surah.number}`} className="surah-card">
      <div className="surah-card__number">{surah.number}</div>
      <div className="surah-card__main">
        <div className="surah-card__name-tr">{surah.nameTurkish}</div>
        <div className="surah-card__name-translit">
          {surah.nameTransliteration}
        </div>
      </div>
      <div className="surah-card__meta">
        <div className="surah-card__name-ar" lang="ar" dir="rtl">
          {surah.nameArabic}
        </div>
        <div className="surah-card__count">
          {PLACE_LABELS[surah.revelationPlace]} · {surah.verseCount} ayet
        </div>
      </div>
    </Link>
  );
}
