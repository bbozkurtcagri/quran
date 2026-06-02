# QuranCompanion · Web UI

Minimal React + Vite + TypeScript client for the QuranCompanion API. Serves
three pages — surah list, surah detail (Arabic + Elmalılı meal), and meal
search.

## Setup

```bash
cd apps/web
npm install
cp .env.example .env.local        # adjust if API is on a different host
npm run dev
# → http://localhost:5173
```

The API base URL is read from `VITE_API_BASE_URL`. Defaults:

| Scenario                | Set this                         |
| ----------------------- | -------------------------------- |
| Host-side `dotnet run`  | `http://localhost:5185`          |
| `docker compose up api` | `http://localhost:8085`          |

## Scripts

| Command            | What it does                          |
| ------------------ | ------------------------------------- |
| `npm run dev`      | Vite dev server on port 5173          |
| `npm run build`    | Type-check + production bundle        |
| `npm run preview`  | Serve the production bundle locally   |
| `npm run lint`     | ESLint                                |

## Project structure

```
src/
├── api/
│   ├── client.ts          fetch wrapper + ApiResponse envelope unwrap
│   └── types.ts           types mirroring backend DTOs
├── components/
│   ├── Layout.tsx         header + nav + outlet
│   ├── SurahCard.tsx      grid item on the list page
│   └── VerseCard.tsx      arabic + meal block
├── pages/
│   ├── SurahListPage.tsx     /              (114 surahs)
│   ├── SurahDetailPage.tsx   /surahs/:n     (verses + meal)
│   └── SearchPage.tsx        /search?q=…    (meal search)
├── App.tsx                router setup
├── main.tsx               React entry
└── styles.css             vanilla CSS, dark theme
```

No state-management library, no UI kit. Routing via `react-router-dom`.
