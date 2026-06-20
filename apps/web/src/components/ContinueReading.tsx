import { Link } from "react-router-dom";
import type { LastRead } from "../hooks/useLastRead";
import { Eyebrow } from "./Eyebrow";

interface Props {
  lastRead: LastRead;
}

export function ContinueReading({ lastRead }: Props) {
  const href = `/surahs/${lastRead.surahNumber}#v${lastRead.verseNumber}`;

  return (
    <Link
      to={href}
      className="group block rounded-2xl border border-border bg-accent-soft/30 hover:bg-accent-soft/60 transition-colors duration-200 ease-[var(--ease-skill)] focus-visible:bg-accent-soft/60"
    >
      <div className="flex items-center justify-between gap-6 px-6 py-5">
        <div className="min-w-0">
          <Eyebrow className="mb-2">Devam et</Eyebrow>
          <p className="font-serif text-[22px] md:text-2xl leading-tight tracking-tight optical-display truncate">
            {lastRead.surahNameTurkish}
            <span className="text-text-muted font-normal">
              {" "}
              · ayet {lastRead.verseNumber}
            </span>
          </p>
        </div>
        <span
          aria-hidden
          className="font-mono text-base text-accent transition-transform duration-200 ease-[var(--ease-skill)] group-hover:translate-x-1 group-focus-visible:translate-x-1"
        >
          →
        </span>
      </div>
    </Link>
  );
}
