import { NavLink, Outlet } from "react-router-dom";
import { useTheme } from "../hooks/useTheme";

export function Layout() {
  const { theme, toggle } = useTheme();

  return (
    <div className="min-h-[100dvh] flex flex-col">
      <header className="sticky top-0 z-20">
        <div className="mx-auto max-w-6xl px-6 pt-6">
          <div className="flex items-center justify-between rounded-full border border-border bg-bg-elev/85 px-5 py-2.5 backdrop-blur-md">
            <NavLink to="/" className="font-serif italic text-[19px] leading-none tracking-tight">
              Kur&apos;an Meali
            </NavLink>
            <nav className="flex items-center gap-1.5">
              <NavTab to="/" label="Sureler" end />
              <NavTab to="/search" label="Ara" />
              <ThemeToggle theme={theme} onToggle={toggle} />
            </nav>
          </div>
        </div>
      </header>

      <main className="flex-1">
        <Outlet />
      </main>

      <footer className="mt-24 border-t border-border">
        <div className="mx-auto max-w-6xl px-6 py-8 flex items-center justify-between text-[11px] uppercase tracking-[0.2em] font-mono text-text-muted">
          <span>Elmalılı Hamdi Yazır · Meal</span>
          <span>Tanzil · Uthmani Minimal</span>
        </div>
      </footer>
    </div>
  );
}

function NavTab({ to, label, end }: { to: string; label: string; end?: boolean }) {
  return (
    <NavLink
      to={to}
      end={end}
      className={({ isActive }) =>
        "px-3 py-1.5 text-sm rounded-full transition-colors duration-200 ease-[var(--ease-skill)] " +
        (isActive
          ? "bg-accent-soft text-accent"
          : "text-text-muted hover:text-text")
      }
    >
      {label}
    </NavLink>
  );
}

function ThemeToggle({ theme, onToggle }: { theme: "light" | "dark"; onToggle: () => void }) {
  const isDark = theme === "dark";
  return (
    <button
      type="button"
      onClick={onToggle}
      aria-label={isDark ? "Aydınlık temaya geç" : "Karanlık temaya geç"}
      className="ml-1 grid place-items-center w-8 h-8 rounded-full text-text-muted hover:text-text hover:bg-accent-soft transition-colors duration-200 ease-[var(--ease-skill)]"
    >
      {isDark ? (
        // sun
        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
          <circle cx="12" cy="12" r="3.5" />
          <path d="M12 3v1.8M12 19.2V21M3 12h1.8M19.2 12H21M5.6 5.6l1.3 1.3M17.1 17.1l1.3 1.3M5.6 18.4l1.3-1.3M17.1 6.9l1.3-1.3" />
        </svg>
      ) : (
        // moon
        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
          <path d="M20 14.5A8 8 0 0 1 9.5 4a8 8 0 1 0 10.5 10.5Z" />
        </svg>
      )}
    </button>
  );
}
