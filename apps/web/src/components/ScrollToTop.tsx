import { useEffect } from "react";
import { useLocation } from "react-router-dom";

/**
 * Rota değişiminde sayfayı başa al — React Router default'ta scroll
 * pozisyonunu korur, bu da yeni route'a "ortadan" girmek gibi hissettirir.
 *
 * Hash navigation'ları (örn. /surahs/2#v34) atlanır; o akışta hedef sayfa
 * kendi içinde `scrollIntoView` ile ayete iner — burada en başa atmak onu bozar.
 */
export function ScrollToTop() {
  const { pathname, hash } = useLocation();

  useEffect(() => {
    if (hash) return;
    window.scrollTo(0, 0);
  }, [pathname, hash]);

  return null;
}
