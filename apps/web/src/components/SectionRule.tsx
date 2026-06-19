/**
 * Decorative editorial rule used between hero and reading column.
 * Two hairlines with an accent dot — soft-skill style, no sharp dividers.
 */
export function SectionRule() {
  return (
    <div className="flex items-center justify-center gap-4 text-text-muted">
      <div className="h-px w-12 bg-border" />
      <span className="text-accent text-xs select-none" aria-hidden>
        ◆
      </span>
      <div className="h-px w-12 bg-border" />
    </div>
  );
}
