import { useCallback, useEffect, useState } from "react";

export interface LastRead {
  surahNumber: number;
  surahNameTurkish: string;
  verseNumber: number;
  at: number;
}

const STORAGE_KEY = "qc-last-read";

function read(): LastRead | null {
  if (typeof window === "undefined") return null;
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as LastRead;
    if (
      typeof parsed?.surahNumber === "number" &&
      typeof parsed?.verseNumber === "number" &&
      typeof parsed?.surahNameTurkish === "string"
    ) {
      return parsed;
    }
    return null;
  } catch {
    return null;
  }
}

/**
 * LocalStorage-backed last-read marker. No auth, no server round-trip.
 * Updates propagate across tabs via the `storage` event.
 */
export function useLastRead() {
  const [lastRead, setLastRead] = useState<LastRead | null>(read);

  useEffect(() => {
    function onStorage(e: StorageEvent) {
      if (e.key !== STORAGE_KEY) return;
      setLastRead(read());
    }
    window.addEventListener("storage", onStorage);
    return () => window.removeEventListener("storage", onStorage);
  }, []);

  const save = useCallback(
    (entry: Omit<LastRead, "at">) => {
      const next: LastRead = { ...entry, at: Date.now() };
      setLastRead(next);
      try {
        window.localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
      } catch {
        // private mode or quota — ignore
      }
    },
    [],
  );

  const clear = useCallback(() => {
    setLastRead(null);
    try {
      window.localStorage.removeItem(STORAGE_KEY);
    } catch {
      // ignore
    }
  }, []);

  return { lastRead, save, clear };
}
