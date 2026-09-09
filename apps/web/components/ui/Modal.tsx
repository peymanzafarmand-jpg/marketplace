"use client";

import { useEffect, useRef } from "react";
import type { ReactNode } from "react";
import { createPortal } from "react-dom";
import { X } from "lucide-react";
import { cn } from "@/lib/utils/cn";

export interface ModalProps {
  open: boolean;
  onClose: () => void;
  title: string;
  description?: string;
  children?: ReactNode;
  className?: string;
}

export function Modal({ open, onClose, title, description, children, className }: ModalProps) {
  const dialogRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    document.addEventListener("keydown", onKeyDown);
    dialogRef.current?.focus();
    document.body.style.overflow = "hidden";
    return () => {
      document.removeEventListener("keydown", onKeyDown);
      document.body.style.overflow = "";
    };
  }, [open, onClose]);

  if (!open || typeof document === "undefined") return null;

  return createPortal(
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div
        className="absolute inset-0 bg-[var(--color-ink-900)]/50"
        onClick={onClose}
        aria-hidden="true"
      />
      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-title"
        aria-describedby={description ? "modal-description" : undefined}
        tabIndex={-1}
        className={cn(
          "relative z-10 w-full max-w-md rounded-[var(--radius-lg)] bg-[var(--color-ink-0)] p-5 shadow-[var(--shadow-popover)] outline-none",
          className
        )}
      >
        <div className="mb-3 flex items-start justify-between gap-4">
          <h2 id="modal-title" className="text-[var(--text-h3)] font-bold text-[var(--color-ink-900)]">
            {title}
          </h2>
          <button
            type="button"
            onClick={onClose}
            aria-label="بستن"
            className="rounded-[var(--radius-sm)] p-1 text-[var(--color-ink-500)] hover:bg-[var(--color-ink-100)]"
          >
            <X className="size-5" aria-hidden="true" />
          </button>
        </div>
        {description && (
          <p id="modal-description" className="mb-3 text-[var(--text-body-sm)] text-[var(--color-ink-600)]">
            {description}
          </p>
        )}
        {children}
      </div>
    </div>,
    document.body
  );
}
