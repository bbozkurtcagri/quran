# Data sources — license and selection

Status: **Decided 2026-05-26.** Revisit only if our distribution model changes
(e.g. moving from a free, non-monetized service to a paid/commercial one).

This document compares six candidate sources for the Arabic Quran text and
Turkish meal data, evaluates them against our constraints, and records the
final choices. We import all chosen data into PostgreSQL; **no external API is
called at runtime**.

## Our constraints

1. The app's primary data is the Quran text and its meal. If it breaks, the app
   has nothing to show. Therefore: **no runtime dependency on a third-party
   API.** APIs may be used only as one-off import bridges.
2. Distribution model **today is non-commercial / ad-free.** A future move to
   monetization is possible; sources whose license forbids commercial use are
   acceptable only if we are prepared to swap them at that point.
3. Every record we serve must carry attribution and a license string back to
   the upstream source. We already model this on `quran.translation_sources`
   (`license_info`, `source_url`).
4. Religious-text content must be **verbatim**. Sources that require us to
   serve the text unmodified are a hard requirement we already meet.

## Source comparison

| Source                  | Data type                    | License                                          | Commercial? | Runtime dep OK? | DB import OK? | Format(s)               | Phase 1 | Phase 2 |
| ----------------------- | ---------------------------- | ------------------------------------------------ | ----------- | --------------- | ------------- | ----------------------- | ------- | ------- |
| **Tanzil**              | Arabic mushaf text (multiple recensions) | Verbatim copies allowed; modification prohibited; attribution + link to tanzil.net required | Yes (implied — "any website or application") | No  | **Yes**       | Text, XML, SQL dump     | **✅ Primary Arabic** | ✅ |
| **Diyanet meal** (kuran.diyanet.gov.tr) | Turkish meal (official) | **Not stated.** Diyanet has an API service (`acikkaynakkuran-dev.diyanet.gov.tr`) gated by GitHub login. No bulk download. No redistribution permission published. | Unclear | No  | **Risky**     | API only (auth required) | ⚠️ Avoid until permission obtained | ⚠️ |
| **Elmalılı Hamdi Yazır** (meal portion of *Hak Dini Kur'an Dili*) | Turkish meal | **Public domain in Türkiye.** Yazır died 27 May 1942; under FSEK Madde 27 (70-year post-mortem), protection ended 31 Dec 2012, public domain since **1 Jan 2013**. | Yes        | n/a              | **Yes**       | Plain text from PD digitizations (Internet Archive, Yazma Eserler Kurumu editions for reference) | **✅ Primary Turkish meal** | ✅ |
| **Quran.com / Quran Foundation API** (api.quran.com v4) | Multiple translations incl. Turkish | Per-translation; the API aggregates third-party works whose individual licenses vary. ToS not explicit on redistribution of underlying text. | Per source  | No (avoid)       | **Per source** | JSON REST               | ⚠️ Use only as import bridge for items already in PD or CC | ⚠️ |
| **AlQuran Cloud API**   | Multiple translations        | Inherits Tanzil for Arabic; per-translation for meals. API itself is open access. | Per source  | No (avoid)       | **Per source** | JSON REST               | ⚠️ Same caveat as Quran.com | ⚠️ |
| **Açık Kuran API** (acik-kuran/acikkuran-api) | Turkish meals (multiple) + word-level data | **CC BY-NC-SA 4.0** | **No (NC clause)** | No  | Limited (NC-only) | JSON REST + repo data    | ⚠️ Only if we accept NC | ⚠️ |

### Source-by-source detail

#### 1. Tanzil — Arabic text

- **Data:** Six text recensions; we want **Uthmani Minimal** (matches Medina
  mushaf, minimal symbols — cleanest for both display and search-after-
  normalization).
- **License (verbatim from tanzil.net/download/):** "Permission is granted to
  copy and distribute verbatim copies of the Quran text provided here" with
  attribution to the Tanzil Project and a link to `tanzil.net`. *Modifying the
  text is not allowed.* No registration required.
- **Format:** Plain text (one ayah per line, `surah|ayah|text`), XML, MySQL
  dump. Current text release: **version 1.1, February 2021**.
- **Pros:** Stable, vetted, widely used in academic and software projects,
  one-time download.
- **Risks:** Must not modify; must keep attribution surfaced. We already store
  this on `translation_sources` records and surface via `/api/v1/translation-sources`.
- **Decision:** **Primary Arabic source.** Imported once, kept verbatim.

#### 2. Diyanet meal

- **Data:** Diyanet İşleri Başkanlığı meal text via `acikkaynakkuran-dev.diyanet.gov.tr`
  API (GitHub-login gated key) and via the open-source frontend
  `diyanet-bid/Kuran` (which fetches at runtime).
- **License:** Not published as open. The API requires auth; redistribution
  rights are not granted.
- **Pros:** Official, authoritative.
- **Risks:** Without explicit permission, importing and serving the text from
  our DB is a copyright risk. Their own open-source frontend fetches at
  runtime — that does not imply redistribution rights for third parties.
- **Decision:** **Do not import as canonical data** until we have written
  permission from Diyanet. We can offer it as a Phase 2 option behind a
  config flag if such permission is obtained.

#### 3. Elmalılı Hamdi Yazır — Turkish meal

- **Data:** The meal (translation) portion of *Hak Dini Kur'an Dili* (1935–1938
  publication; meal extracted from the multi-volume tafsir).
- **License:** **Public domain in Türkiye** since 1 January 2013 (Yazır d. 1942,
  FSEK Art. 27 — 70 years post-mortem). Modern *typeset editions* (e.g.
  Yazma Eserler Kurumu Başkanlığı's reprint) may carry an *editorial*
  copyright; we use the **plain meal text only**, not their typographical
  arrangement, footnotes or tafsir prose.
- **Format:** Multiple Internet Archive digitizations exist; cleanest source
  for the **meal-only** text is to extract from a verified digital edition
  (e.g. the Quran.com data set for translator ID 52, cross-checked against
  Internet Archive scans).
- **Pros:** No copyright risk in Türkiye, no NC clause, well-known and
  respected translator.
- **Risks:**
  - Some digitizations contain editor's modernizations (Latin transcriptions,
    parenthetical glosses). We need a clean meal-only version per ayah.
  - International distribution: protection terms differ by country; if we
    ever ship globally and a jurisdiction has a longer term, that's a future
    concern. Türkiye-domiciled service is safe.
- **Decision:** **Default Turkish meal.** Imported once from a verified
  source (preference: cross-check Quran.com translator ID 52 against the
  Internet Archive scan).

#### 4. Quran.com / Quran Foundation API

- **Data:** Arabic + 200+ translations + tafsir + audio + word-by-word.
- **License:** **Per translator.** The API itself aggregates third-party
  works; redistribution rights depend on the individual translator's terms.
  Public ToS does not grant blanket self-hosting rights.
- **Pros:** Best-in-class data quality, well-maintained, four Turkish
  translations: Diyanet (77), Elmalılı (52), Muslim Shahin (124), Shaban
  Britch (112).
- **Risks:** Using as a runtime dependency violates our constraint #1. Using
  as an import bridge is safe **only for the translations whose underlying
  licenses we have verified independently**.
- **Decision:** **Import bridge only,** and only for the Elmalılı entry (ID 52
  — PD in Türkiye). We do not import Diyanet (77) without separate permission.

#### 5. AlQuran Cloud API

- **Data:** Mirrors Tanzil for Arabic; bundles various translations.
- **License:** API itself is open; underlying data inherits each source's
  license.
- **Pros:** Easy to use, no auth.
- **Risks:** Same per-translation caveat as Quran.com. No advantage over
  going directly to Tanzil + a verified Elmalılı extract.
- **Decision:** **Do not use.** Adds no value over the primary sources.

#### 6. Açık Kuran (acik-kuran/acikkuran-api)

- **Data:** Multiple Turkish meals + word-level data + tafsir excerpts.
- **License:** **CC BY-NC-SA 4.0** — Attribution + NonCommercial + ShareAlike.
- **Pros:** Open source, well-structured, multiple Turkish meals, active
  Turkish community project.
- **Risks:**
  - **NC clause** means any future commercialization (paid app, ad-supported
    distribution) requires removing this data.
  - **SA clause** means derivative data is also CC BY-NC-SA — could constrain
    how we publish our own normalized/search-indexed corpus.
- **Decision:** **Defer.** Not imported in Phase 1. If we want a *second*
  Turkish meal in Phase 2 and remain non-commercial, revisit. Mehmet Okuyan,
  Erhan Aktaş, Bayraktar Bayraklı meals (modern works, not PD) would be
  good candidates if the NC trade-off is acceptable.

## Final decisions

| Decision                          | Choice                                                       |
| --------------------------------- | ------------------------------------------------------------ |
| Arabic mushaf source              | **Tanzil — Uthmani Minimal, version 1.1 (Feb 2021)**         |
| Default Turkish meal              | **Elmalılı Hamdi Yazır (meal-only, public domain in Türkiye)** |
| Secondary meals (Phase 1)         | None.                                                        |
| Runtime API dependency            | **None.** All data imported into Postgres.                  |
| Import bridge for Elmalılı text   | Cross-check Quran.com translator ID 52 against Internet Archive scan, then commit cleaned JSON to `seed-data/translations/elmalili.json`. |
| Diyanet meal                      | **Excluded** until written permission obtained.              |
| Açık Kuran data                   | **Deferred** to Phase 2; revisit only if we stay non-commercial. |

## Seed-file shapes

These match the schemas already documented in [`seed-data/README.md`](../seed-data/README.md);
recorded here for the source-specific values we expect.

### `seed-data/quran/surahs.json`

114 records of factual metadata (number, name, verse count, revelation place).
**Not copyrightable** — pure fact. Compiled by us from the standard mushaf.

### `seed-data/quran/verses.json`

6,236 records keyed by `(surahNumber, verseNumber)` with `arabicText` taken
verbatim from Tanzil's Uthmani Minimal release.

### `seed-data/translations/sources.json`

Two records on first import:

```json
[
  {
    "code": "tanzil-uthmani-minimal",
    "name": "Tanzil — Uthmani Minimal",
    "languageCode": "ar",
    "author": "Tanzil Project",
    "description": "Arabic mushaf text, Uthmani minimal recension.",
    "licenseInfo": "Verbatim distribution permitted. Modification prohibited. Attribution to Tanzil Project (https://tanzil.net) required.",
    "sourceUrl": "https://tanzil.net/download/",
    "isDefault": false,
    "isActive": true
  },
  {
    "code": "elmalili",
    "name": "Elmalılı Hamdi Yazır — Meal",
    "languageCode": "tr",
    "author": "Elmalılı Muhammed Hamdi Yazır",
    "description": "Hak Dini Kur'an Dili — meal portion (1935–1938).",
    "licenseInfo": "Public domain in Türkiye since 1 January 2013 (FSEK Art. 27).",
    "sourceUrl": "https://archive.org/details/ElmaliliKuranTefsiri",
    "isDefault": true,
    "isActive": true
  }
]
```

### `seed-data/translations/elmalili.json`

6,236 ayah meals, format already defined in `seed-data/README.md`.

## Schema notes

The Arabic text source needs metadata-level attribution too, but currently
`Verse.ArabicText` carries no source FK. **Two viable options:**

1. **Store the Arabic source as a row in `quran.translation_sources` with
   `language_code = "ar"`,** then surface it via `/api/v1/translation-sources?languageCode=ar`.
   No schema change needed. `Verse.ArabicText` stays denormalized for read
   performance; the source attribution is fetched separately. **Recommended.**

2. Add an `ArabicTextSourceId` FK on `Verse`. Strictly more correct but adds a
   migration and a join per verse fetch for marginal benefit (we only have one
   Arabic source).

**Decision:** Option 1. No migration needed. We just import the Tanzil row
into the existing `translation_sources` table.

## Phase 2 implications

When question-answering work begins, this document is the canonical answer to
"can we feed translation X to the LLM as evidence?" For sources we don't own
attribution rights to, we cannot include their text in prompts that get
persisted (e.g. in `qa.generated_answers` citations). The Elmalılı/PD path
keeps this clean. If we ever want to cite Diyanet or modern works in answers,
we need licensing in place first.

## Implementation notes

### Bismillah handling (Arabic text import)

When importing via the AlQuran Cloud bridge (which republishes Tanzil's
`quran-uthmani-min` edition), the bismillah is prepended **inline** to verse 1
of every sura except Al-Fatihah (where it *is* verse 1) and At-Tawbah (which
has no bismillah). The Tanzil source text itself does not store bismillah
inline. The seed importer strips this prefix so each verse stores only its
own text:

- Al-Fatihah v1 → `بِسمِ اللَّهِ الرَّحمٰنِ الرَّحيمِ` (unchanged — this is the verse)
- Al-Baqarah v1 → `الم` (bismillah stripped)
- Al-Ikhlas v1 → `قُل هُوَ اللَّهُ أَحَدٌ` (bismillah stripped)
- At-Tawbah v1 → unchanged (no bismillah to begin with)

### Unicode normalization gotcha

Tanzil's text orders combining marks as **shadda (U+0651) before fatha (U+064E)**
in some words (`اللَّهِ`); a literal typed in source code or a different
distribution may use the opposite order. They render identically but are
byte-different. Comparison code that needs to detect the bismillah prefix
extracts it from the data itself (Al-Fatihah v1) rather than hard-coding a
literal.

## Open follow-ups

- [x] Download Tanzil Uthmani-Minimal 1.1 plain text — via AlQuran Cloud
      `quran-uthmani-min` edition, 2026-05-26.
- [x] Pull a clean Elmalılı meal-only dataset — via Quran.com translator
      ID 52 (`/api/v4/quran/translations/52`), 2026-05-26. 6,236 records
      joined to verse coordinates via the local `verses.json`. Spot-checked
      Al-Fatihah, Al-Baqarah, At-Tawbah, Al-Ikhlas, An-Nas. Full Internet
      Archive cross-check remains an open item if anyone hits a suspect
      verse.
- [x] Register `seed-data/translations/sources.json` (committed). The actual
      `elmalili.json` (1.4 MB) stays gitignored alongside `verses.json`.
- [x] Attribution strings on `translation_sources` records match this
      document's "Final decisions" section verbatim.
