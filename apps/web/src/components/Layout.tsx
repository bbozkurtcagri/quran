import { NavLink, Outlet } from "react-router-dom";

export function Layout() {
  return (
    <div className="app">
      <header className="app-header">
        <div className="app-header__inner">
          <NavLink to="/" className="app-brand">
            QuranCompanion
          </NavLink>
          <nav className="app-nav">
            <NavLink
              to="/"
              end
              className={({ isActive }) =>
                "app-nav__link" + (isActive ? " is-active" : "")
              }
            >
              Sureler
            </NavLink>
            <NavLink
              to="/search"
              className={({ isActive }) =>
                "app-nav__link" + (isActive ? " is-active" : "")
              }
            >
              Ara
            </NavLink>
          </nav>
        </div>
      </header>
      <main className="app-main">
        <Outlet />
      </main>
      <footer className="app-footer">
        <span>Elmalılı meali · Tanzil mushafı</span>
      </footer>
    </div>
  );
}
