"use client";

import { useEffect } from "react";
import type { ReactNode } from "react";
import { createPortal } from "react-dom";
import { X } from "lucide-react";
import { cn } from "@/lib/utils/cn";

export interface DrawerProps {
  open: boolean;
  onClose: () => void;
  title: string;
  children?: ReactNode;
  /** "bottom" is the default — the natural mobile pattern (filters, cart quick view). */
  side?: "bottom" | "end";
  className?: string;
}

export function Drawer({ open, onClose, title, children, side = "bottom", className }: DrawerProps) {
  useEffect(() => {
    if (!open) return;
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    document.addEventListener("keydown", onKeyDown);
    document.body.style.overflow = "hidden";
    return () => {
      document.removeEventListener("keydown", onKeyDown);
      document.body.style.overflow = "";
    };
  }, [open, onClose]);

  if (!open || typeof document === "undefined") return null;

  const isBottom = side === "bottom";

  return createPortal(
    <div className="fixed inset-0 z-50 flex">
      <div className="absolute inset-0 bg-[var(--color-ink-900)]/50" onClick={onClose} aria-hidden="true" />
      <div
        role="dialog"
        aria-modal="true"
        aria-label={title}
        className={cn(
          "relative z-10 bg-[var(--color-ink-0)] shadow-[var(--shadow-popover)]",
          isBottom
            ? "mt-auto w-full rounded-t-[var(--radius-lg)] max-h-[85vh] overflow-y-auto"
            : "me-0 ms-auto h-full w-full max-w-sm overflow-y-auto",
          className
        )}
      >
        <div className="sticky top-0 flex items-center justify-between border-b border-[var(--color-ink-200)] bg-[var(--color-ink-0)] px-4 py-3">
          <h2 className="text-[var(--text-h3)] font-bold text-[var(--color-ink-900)]">{title}</h2>
          <button
            type="button"
            onClick={onClose}
            aria-label="بستن"
            className="rounded-[var(--radius-sm)] p-1 text-[var(--color-ink-500)] hover:bg-[var(--color-ink-100)]"
          >
            <X className="size-5" aria-hidden="true" />
          </button>
        </div>
        <div className="p-4">{children}</div>
      </div>
    </div>,
    document.body
  );
}
