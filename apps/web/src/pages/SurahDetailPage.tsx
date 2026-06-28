import { useEffect, useRef, useState } from "react";
import { Link, useLocation, useParams } from "react-router-dom";
import { getSurahDetail, getSurahVerses } from "../api/client";
import type { SurahDetail, VerseSummary } from "../api/types";
import { Eyebrow } from "../components/Eyebrow";
import { SectionRule } from "../components/SectionRule";
import { Verse } from "../components/Verse";
import { useLastRead } from "../hooks/useLastRead";

const PLACE_LABEL: Record<SurahDetail["revelationPlace"], string> = {
  Meccan: "Mekkî",
  Medinan: "Medenî",
  Unknown: "—",
};

export function SurahDetailPage() {
  const { number } = useParams<{ number: string }>();
  const { hash } = useLocation();
  const surahNumber = number ? parseInt(number, 10) : NaN;
  // URL'de #vN varsa — search/devam-et akışlarında — bu ayete scroll edilecek hedef.
  const targetVerse = (() => {
    const m = hash.match(/^#v(\d+)$/);
    return m ? parseInt(m[1], 10) : null;
  })();

  const [surah, setSurah] = useState<SurahDetail | null>(null);
  const [verses, setVerses] = useState<VerseSummary[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  // En üstte (sticky navbar'ın altında) görünen ayet. IntersectionObserver
  // buraya yazar, save tetiklenir.
  const [topmostVerse, setTopmostVerse] = useState<number | null>(null);
  // Aynı verseNumber'a iki kere yazıp localStorage'a gereksiz IO yapmamak için
  // son save'i hatırlarız.
  const lastSavedVerse = useRef<number | null>(null);

  const { save: saveLastRead } = useLastRead();

  useEffect(() => {
    if (Number.isNaN(surahNumber)) {
      setError("Geçersiz sure numarası.");
      return;
    }

    const ctrl = new AbortController();
    setSurah(null);
    setVerses(null);
    setError(null);

    Promise.all([
      getSurahDetail(surahNumber, ctrl.signal),
      getSurahVerses(surahNumber, undefined, 1, 300, ctrl.signal),
    ])
      .then(([s, v]) => {
        setSurah(s);
        setVerses(v.items);
      })
      .catch((err) => {
        if (err.name !== "AbortError") {
          setError(err.message ?? "Bilinmeyen hata");
        }
      });

    return () => ctrl.abort();
  }, [surahNumber]);

  // Scroll-driven last-read. Verse component'ları DOM'da `id="vN"` ile var;
  // IntersectionObserver üst banta giren ayetleri yakalar, en küçük numara
  // o anki "okunan" ayettir.
  useEffect(() => {
    if (!verses || verses.length === 0) return;

    // rootMargin üst banttan -120px (sticky offset), alttan -55% (sadece
    // üst yarıyı tracker olarak izle). Bant içine giren ilk verse topmost'tur.
    const observer = new IntersectionObserver(
      (entries) => {
        const visibleNumbers = entries
          .filter((e) => e.isIntersecting)
          .map((e) => parseInt(e.target.id.slice(1), 10))
          .filter((n) => !Number.isNaN(n));
        if (visibleNumbers.length > 0) {
          setTopmostVerse(Math.min(...visibleNumbers));
        }
      },
      { rootMargin: "-120px 0px -55% 0px", threshold: 0 },
    );

    verses.forEach((v) => {
      const el = document.getElementById(`v${v.verseNumber}`);
      if (el) observer.observe(el);
    });

    return () => observer.disconnect();
  }, [verses]);

  // Verses geldikten sonra hedef ayete (varsa) tek seferlik programatik scroll.
  // SPA olduğumuz için browser native hash-scroll DOM hazır olmadan tetikleniyor
  // ve atlıyor — bu efekt onu telafi eder.
  useEffect(() => {
    if (!verses || verses.length === 0) return;
    if (targetVerse == null) return;
    // requestAnimationFrame ile bir tick beklet ki layout tamamlansın.
    const id = requestAnimationFrame(() => {
      const el = document.getElementById(`v${targetVerse}`);
      el?.scrollIntoView({ block: "start", behavior: "smooth" });
    });
    return () => cancelAnimationFrame(id);
  }, [verses, targetVerse]);

  // Sure yüklendiğinde ilk save'i de yap — kullanıcı hiç scroll etmeden çıksa
  // bile son okuduğu sure listede chip olarak görünür. Hash hedefi varsa onu
  // baz al; yoksa scroll observer'ın ilk değerini ya da 1'i kullan.
  useEffect(() => {
    if (!surah) return;
    if (lastSavedVerse.current != null) return;
    const initial = targetVerse ?? topmostVerse ?? 1;
    saveLastRead({
      surahNumber: surah.number,
      surahNameTurkish: surah.nameTurkish,
      verseNumber: initial,
    });
    lastSavedVerse.current = initial;
  }, [surah, targetVerse, topmostVerse, saveLastRead]);

  // Topmost verse değiştikçe last-read'i güncelle.
  useEffect(() => {
    if (!surah || topmostVerse == null) return;
    if (topmostVerse === lastSavedVerse.current) return;
    saveLastRead({
      surahNumber: surah.number,
      surahNameTurkish: surah.nameTurkish,
      verseNumber: topmostVerse,
    });
    lastSavedVerse.current = topmostVerse;
  }, [surah, topmostVerse, saveLastRead]);

  if (error) {
    return (
      <div className="mx-auto max-w-2xl px-6 py-32 text-center">
        <Eyebrow className="mb-4">Hata</Eyebrow>
        <p className="font-reading text-lg text-text-muted">{error}</p>
        <Link
          to="/"
          className="mt-8 inline-block font-mono text-[11px] uppercase tracking-[0.22em] text-accent hover:underline"
        >
          ← Sure listesine dön
        </Link>
      </div>
    );
  }

  if (!surah || !verses) {
    return (
      <div className="my-32 text-center font-mono text-xs uppercase tracking-[0.22em] text-text-muted">
        Yükleniyor
      </div>
    );
  }

  return (
    <article className="mx-auto max-w-3xl px-6">
      <div className="pt-12">
        <Link
          to="/"
          className="font-mono text-[11px] uppercase tracking-[0.22em] text-text-muted hover:text-text transition-colors duration-200 ease-[var(--ease-skill)]"
        >
          ← Sureler
        </Link>
      </div>

      <header className="pt-16 pb-20 md:pt-24 md:pb-24 text-center">
        <Eyebrow className="mb-6">
          Sure {surah.number} · {PLACE_LABEL[surah.revelationPlace]} · {surah.verseCount} ayet
        </Eyebrow>
        <h1 className="font-serif text-6xl md:text-7xl leading-[1.05] tracking-tight optical-display">
          {surah.nameTurkish}
        </h1>
        <p
          lang="ar"
          dir="rtl"
          className="font-arabic text-5xl md:text-6xl text-accent mt-6"
        >
          {surah.nameArabic}
        </p>
        <p className="mt-3 font-mono text-xs uppercase tracking-[0.22em] text-text-muted">
          {surah.nameTransliteration}
        </p>
        <div className="mt-12">
          <SectionRule />
        </div>
      </header>

      <div className="pb-32">
        {verses.map((v) => (
          <Verse key={v.globalVerseNumber} verse={v} />
        ))}
      </div>
    </article>
  );
}
