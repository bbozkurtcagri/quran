import { type ReactNode } from "react";

/**
 * Small, all-caps, wide-tracked monospace label — the eyebrow pattern from
 * the taste-skill spec. Use for metadata above headings and on cards.
 */
export function Eyebrow({
  children,
  className = "",
}: {
  children: ReactNode;
  className?: string;
}) {
  return (
    <p
      className={
        "font-mono text-[10px] uppercase tracking-[0.22em] text-text-muted " + className
      }
    >
      {children}
    </p>
  );
}
