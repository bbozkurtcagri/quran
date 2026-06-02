// Shapes here mirror the backend DTOs at QuranCompanion.Application.Features.*.
// Keep them updated when the backend contract changes.

export interface ApiResponse<T> {
  data: T | null;
  success: boolean;
  message: string | null;
  errors: ApiError[];
}

export interface ApiError {
  code: string;
  message: string;
  field?: string | null;
}

export interface SurahListItem {
  number: number;
  nameArabic: string;
  nameTurkish: string;
  nameTransliteration: string;
  verseCount: number;
  revelationPlace: "Meccan" | "Medinan" | "Unknown";
}

export interface SurahDetail extends SurahListItem {
  displayOrder: number;
}

export interface Translation {
  sourceCode: string;
  sourceName: string;
  languageCode: string;
  author: string;
  text: string;
}

export interface VerseSummary {
  surahNumber: number;
  verseNumber: number;
  globalVerseNumber: number;
  juzNumber: number;
  pageNumber: number;
  arabicText: string;
  translations: Translation[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface VerseSearchHit {
  surahNumber: number;
  surahNameTurkish: string;
  verseNumber: number;
  globalVerseNumber: number;
  arabicText: string;
  translationSourceCode: string;
  translationText: string;
}
