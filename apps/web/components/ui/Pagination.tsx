import { ChevronRight, ChevronLeft } from "lucide-react";
import { cn } from "@/lib/utils/cn";

export interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  className?: string;
}

function getPageList(current: number, total: number): (number | "ellipsis")[] {
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
  const pages = new Set<number>([1, total, current, current - 1, current + 1]);
  const sorted = [...pages].filter((p) => p >= 1 && p <= total).sort((a, b) => a - b);
  const result: (number | "ellipsis")[] = [];
  sorted.forEach((page, i) => {
    if (i > 0 && page - sorted[i - 1] > 1) result.push("ellipsis");
    result.push(page);
  });
  return result;
}

export function Pagination({ currentPage, totalPages, onPageChange, className }: PaginationProps) {
  const pages = getPageList(currentPage, totalPages);

  return (
    <nav aria-label="صفحه‌بندی" className={cn("flex items-center justify-center gap-1", className)}>
      <button
        type="button"
        aria-label="صفحه قبل"
        disabled={currentPage === 1}
        onClick={() => onPageChange(currentPage - 1)}
        className="grid size-9 place-items-center rounded-[var(--radius-md)] text-[var(--color-ink-700)] hover:bg-[var(--color-ink-100)] disabled:opacity-40"
      >
        {/* RTL: "previous" points to the right */}
        <ChevronRight className="size-4" aria-hidden="true" />
      </button>

      {pages.map((page, i) =>
        page === "ellipsis" ? (
          <span key={`ellipsis-${i}`} className="px-1 text-[var(--color-ink-400)]">
            …
          </span>
        ) : (
          <button
            key={page}
            type="button"
            aria-current={page === currentPage ? "page" : undefined}
            onClick={() => onPageChange(page)}
            className={cn(
              "grid size-9 place-items-center rounded-[var(--radius-md)] text-[var(--text-body-sm)] font-medium",
              page === currentPage
                ? "bg-[var(--color-brand-500)] text-white"
                : "text-[var(--color-ink-700)] hover:bg-[var(--color-ink-100)]"
            )}
          >
            {page}
          </button>
        )
      )}

      <button
        type="button"
        aria-label="صفحه بعد"
        disabled={currentPage === totalPages}
        onClick={() => onPageChange(currentPage + 1)}
        className="grid size-9 place-items-center rounded-[var(--radius-md)] text-[var(--color-ink-700)] hover:bg-[var(--color-ink-100)] disabled:opacity-40"
      >
        <ChevronLeft className="size-4" aria-hidden="true" />
      </button>
    </nav>
  );
}
