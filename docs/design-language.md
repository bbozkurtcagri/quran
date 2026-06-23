# Design language

Single source of truth for visuals across **web (apps/web)**, **iOS (apps/ios)**
and **Android (apps/android, later)**. The web client is the reference
implementation; iOS and Android target the same numeric values, with
platform-idiomatic wrappers.

Adapted from
[leonxlnx/taste-skill](https://github.com/leonxlnx/taste-skill) — soft
(Editorial Luxury) variant blended with minimalist (Premium Utilitarian).

## Philosophy

1. **Editorial, not dashboard.** Massive serif headings, generous whitespace,
   hairline dividers. No card-everywhere, no gradient hero, no neon, no
   glassmorphism.
2. **Two layers of restraint.** Typography carries the hierarchy; colour
   carries only the accent. Most surfaces use one neutral.
3. **Readable in long sessions.** Warm parchment palette (not pure white,
   not true black). Long line-heights for Arabic, ample meal column width.
4. **Motion is invisible until it isn't.** Single easing curve, no scroll
   listeners, no decorative animation. Respect `prefers-reduced-motion`.

## Banned

- Fonts: **Inter, Roboto, Open Sans, Helvetica, Arial**
- Icons: Lucide, Feather, default Heroicons (heavy strokes)
- Effects: drop shadows above `0.05` opacity, gradients, neon, 3D glass
- Patterns: edge-to-edge dashboard nav, 3-col Bootstrap grids without breathing
  room, emoji in UI copy
- Animation targets: `top/left/width/height` — animate only `transform` and
  `opacity` (or their platform equivalents)

## Colour tokens

Two palettes — light is the default, dark is warm-dark (not true black).

| Token            | Light       | Dark        | Notes                                            |
| ---------------- | ----------- | ----------- | ------------------------------------------------ |
| `bg`             | `#FBFBFA`   | `#1A1612`   | Warm bone / warm dark. Page background.          |
| `bg-elev`        | `#FFFFFF`   | `#221C16`   | Floating surfaces (nav pill, cards if needed).   |
| `text`           | `#111111`   | `#F2E9DA`   | Never `#000000` or pure `#FFFFFF`.               |
| `text-muted`     | `#6B6660`   | `#9E907C`   | Captions, meta, meal body in some contexts.      |
| `border`         | `rgba(0,0,0,0.07)` | `rgba(255,255,255,0.08)` | Hairlines only. No solid 1px grey. |
| `accent`         | `#346538`   | `#7FB88A`   | Muted emerald. The only saturated colour.        |
| `accent-soft`    | `#EDF3EC`   | `rgba(127,184,138,0.08)` | Hover tints, eyebrow chips.        |

### Selection / focus

- Selection background: `accent-soft`; selection foreground: `accent`.
- Focus ring: 2px `accent` with 2px offset, 4px radius. Visible only via
  `:focus-visible` / keyboard.

## Typography

Five faces. **Inter is intentionally absent.**

| Role                                 | Family                                                | Notes                                                |
| ------------------------------------ | ----------------------------------------------------- | ---------------------------------------------------- |
| **Display / brand**                  | `Instrument Serif` (regular + italic)                 | Editorial italic for wordmark, hero H1 in some pages.|
| **Reading body (Turkish meal, prose)** | `Newsreader` (opsz 6-72, 400/500, italic 400)       | Long-form reading. Optical sizing makes large sizes feel tighter naturally. |
| **UI (nav, buttons, controls)**      | `Geist` (400/500/600)                                 | Sans for interface chrome.                            |
| **Eyebrows, meta, numbers**          | `Geist Mono` (400)                                    | Uppercase, wide tracking, 10-12px.                    |
| **Arabic mushaf**                    | `Amiri` (400, 700)                                    | Classical Naskh shape used by Tanzil and most academic editions. |

### Optical guidance

- Hero H1: `Instrument Serif` italic, very large (`text-6xl`–`text-8xl` on web ≈ 60-96 px). Tracking `-0.02em`, line-height `1.05`.
- Body reading: `Newsreader` 17-19 px, line-height `1.7-1.8`.
- Arabic verse: `Amiri` 28-32 px, line-height `2.2-2.4`, RTL.
- Eyebrow: `Geist Mono` 10-11 px, `uppercase`, tracking `0.22em`.
- UI body: `Geist` 14-15 px, line-height `1.5`.
- Headings + display sizes use `font-optical-sizing: auto` (Newsreader).

## Spacing scale

4-px base, exponential.

```
  4 ·  8 · 12 · 16 · 24 · 32 · 48 · 64 · 96 · 128
```

Section padding: 64-96 px desktop, 32 px mobile. Reading column: `max-w-3xl`
(~672 px). Browse column: `max-w-5xl` (~1024 px).

## Radii

- `4` — small chips, focus halo
- `8` — buttons, search input rounding
- `12` — cards / banners (rare; we prefer hairlines)
- `9999` — pills (nav segments, "Devam et" chip)

No `2rem` mega-radii (soft-skill ethereal variant uses them; we don't).

## Motion

Single canonical curve: `cubic-bezier(0.16, 1, 0.3, 1)` (the so-called
*soft-skill ease*). `linear` and `ease-in-out` are banned.

| Pattern             | Duration | Properties                              |
| ------------------- | -------- | --------------------------------------- |
| Hover/focus colour  | 200 ms   | `color`, `background-color`             |
| Hover slide arrow   | 200 ms   | `transform: translateX(2-4px)`, `opacity` |
| Entry fade-in       | 600 ms   | `transform: translateY(12px)`, `opacity` |
| Search input focus  | 300 ms   | `border-color`                          |

Respect `prefers-reduced-motion` at the base layer: clamp all transitions
to `0.01ms` when the user opts out.

## Component patterns

### Eyebrow

Small, all-caps, monospace, wide-tracked label used above every page H1
and as section heads.

```
GEIST MONO · 10 px · UPPERCASE · TRACKING 0.22em · text-muted
```

### Section rule

Decorative divider between hero and reading column.

```
[ hairline 48 px ] [ ◆ accent ] [ hairline 48 px ]
```

### Header (floating nav)

- Sticky to top, **inside max-w-6xl container**, with 24 px top inset.
- Pill: `border border-border bg-bg-elev/85 backdrop-blur-md`, radius
  `9999`, height ~44 px.
- Brand: `Instrument Serif` italic 19 px, tracking tight.
- Tabs: `Geist` 14 px, padding `12px 12px`, rounded-full. Active state
  uses `accent-soft` background + `accent` text.
- Right slot: theme toggle (15×15 px line icons, no fill).

### List row (Surah list)

Editorial typographic block — **never a card box**.

- Padding: `20 24 px`.
- Layout: `[number] [name + eyebrow] [arabic] [→]` four columns.
- Number: `Geist Mono` 12 px muted; turns `accent` on hover.
- Name: `Instrument Serif` 24-28 px tracked tight.
- Eyebrow: `MEKKÎ · 7 AYET · AL-FATIHAH` Geist Mono.
- Arabic: `Amiri` 24-28 px, RTL, gains `accent` colour on hover.
- Hover/focus:
  - Background → `accent-soft / 50%`
  - Left edge: 2 px tall hairline `accent` bar (top/bottom 12 px inset),
    fades in via opacity
  - Right arrow `→` Geist Mono, fades in + slides 8 px on entry
- Divider: `border-b border-border`, last row drops the border.

### Verse block (reading)

No card. Pure typography.

- Container padding: `40 0 px` per verse; `border-b border-border`.
- Header row: large `Instrument Serif` verse number on the left
  (3xl, `accent`, tabular numerals); eyebrow meta on the right
  (`Cüz · Sayfa`).
- Arabic: `Amiri` 28-32 px, line-height 2.2-2.4, RTL.
- Meal: `Newsreader` 18-19 px, line-height 1.75, `text-muted`.
- Vertical gap between Arabic and meal: 32 px.

### Search input

Borderless, alt-çizgili.

- Font: `Newsreader` 24-30 px.
- Padding: `16 0 px`.
- Border: `border-b border-border`, `focus-within: border-accent`.
- Placeholder: italic-y caption in `text-muted/60`.
- 300 ms ease colour transition on focus.

### Mode segmented control (search)

- Container: `inline-flex rounded-full border border-border p-0.5 bg-bg-elev/60`.
- Buttons: `Geist Mono` 12 px uppercase tracked `0.18em`, padding `6 16 px`,
  rounded-full.
- Active state: `accent-soft` background + `accent` text.

### Continue reading chip

- Rounded-2xl container (`12 px` radius), `border border-border`,
  `bg-accent-soft / 30%`.
- Layout: `[eyebrow + serif title] [→ accent arrow]`.
- Hover lifts background to `accent-soft / 60%`.

### About-style prose

- Container: `max-w-3xl`, page padding 24 px.
- Each section: eyebrow + body. `Newsreader` 17 px, line-height 1.8.
- Definition list (`dl`/`dt`/`dd`) for sources: `Instrument Serif`
  19 px term + `Newsreader` muted definition.

## Platform mapping

### Web (`apps/web`)

Tailwind v4 with `@theme` tokens. Implementation: `apps/web/src/theme.css`.
Light/dark via `.dark` class on `<html>`, toggled by `useTheme.ts`.

### iOS (`apps/ios`)

- Asset Catalog colour set per token (light + dark appearances).
- `Font` extensions for each face (`Font.editorialDisplay`, `Font.reading`,
  `Font.ui`, `Font.mono`, `Font.arabic`).
- `Color` extensions: `Color.bg`, `Color.text`, `Color.textMuted`,
  `Color.accent`, `Color.accentSoft`, `Color.border`.
- `Animation.softSkill` = `Animation.timingCurve(0.16, 1, 0.3, 1, duration: 0.2)`.
- Custom fonts bundled in `Resources/Fonts/`, registered in `Info.plist`.
- Theme controlled via `@AppStorage("qc-theme")` + `preferredColorScheme`.

### Android (`apps/android`, later)

- Compose `Color` defined per theme in `ui/theme/Color.kt`.
- `FontFamily` per face in `ui/theme/Type.kt` (fonts bundled in
  `res/font/`).
- `Typography` material override.
- Easing: `CubicBezierEasing(0.16f, 1f, 0.3f, 1f)`.

## Asset bundle (custom fonts)

Web pulls fonts from Google Fonts with `preconnect` + `display=swap`. iOS
and Android bundle them inside the binary so they work offline and look
identical at first paint.

Files to bundle (download from Google Fonts):

```
InstrumentSerif-Regular.ttf
InstrumentSerif-Italic.ttf
Newsreader[opsz,wght].ttf            (variable, regular + italic)
Geist-Regular.ttf
Geist-Medium.ttf
Geist-SemiBold.ttf
GeistMono-Regular.ttf
Amiri-Regular.ttf
Amiri-Bold.ttf
```

Licensing for all of the above: SIL Open Font License 1.1 — free to
bundle in any application.
