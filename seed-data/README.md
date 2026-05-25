# Seed data

The application keeps Quran text, translations and metadata in PostgreSQL. The
seed CLI imports the contents of this directory at startup time and is
idempotent: re-running on identical content is a no-op (tracked by SHA256 hash
in `import.import_histories`).

**No data is checked in.** The actual JSON files are listed in `.gitignore`
because their licensing depends on the source you choose. Populate them locally
following the structure below, then run:

```bash
dotnet run --project src/QuranCompanion.Api -- seed
```

The CLI applies migrations first, then imports in this order:

1. `quran/surahs.json`
2. `quran/verses.json`
3. `translations/sources.json`
4. `translations/<source_code>.json` for each translation source

## File layouts

### `quran/surahs.json`

```json
[
  {
    "number": 1,
    "nameArabic": "الفاتحة",
    "nameTurkish": "Fâtiha",
    "nameTransliteration": "Al-Fatihah",
    "verseCount": 7,
    "revelationPlace": "Meccan",
    "displayOrder": 1
  }
]
```

`revelationPlace` accepts `Meccan`, `Medinan`, or `Unknown`.

### `quran/verses.json`

```json
[
  {
    "surahNumber": 1,
    "verseNumber": 1,
    "globalVerseNumber": 1,
    "juzNumber": 1,
    "pageNumber": 1,
    "arabicText": "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ"
  }
]
```

### `translations/sources.json`

```json
[
  {
    "code": "diyanet",
    "name": "Diyanet Meali",
    "languageCode": "tr",
    "author": "Diyanet İşleri Başkanlığı",
    "description": "Resmi Diyanet meali",
    "licenseInfo": "…",
    "sourceUrl": "https://kuran.diyanet.gov.tr",
    "isDefault": true,
    "isActive": true
  }
]
```

### `translations/<source_code>.json`

```json
{
  "sourceCode": "diyanet",
  "translations": [
    {
      "surahNumber": 1,
      "verseNumber": 1,
      "text": "Rahmân ve Rahîm olan Allah'ın adıyla."
    }
  ]
}
```

`sourceCode` inside the file must match an entry in `translations/sources.json`.
The filename is used only as a hint and for import history bookkeeping.

## Licensing notes

Quran text and meal data are subject to the source's licensing. Before
checking in or distributing seeded data, verify that the license permits it.
Recommended starting points:

- **Arabic text**: [Tanzil](https://tanzil.net) — text version 1.1, non-commercial
  use with attribution; do not modify the text.
- **Turkish meal**: Diyanet (resmi), Elmalılı Hamdi Yazır (public domain in
  Türkiye), or another source whose license you have verified.

Always store the upstream license string in `licenseInfo` on the translation
source record so the API can surface it to clients.
