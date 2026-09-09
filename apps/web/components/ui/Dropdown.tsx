"use client";

import { useEffect, useRef, useState } from "react";
import type { ReactNode } from "react";
import { cn } from "@/lib/utils/cn";

export interface DropdownItem {
  key: string;
  label: string;
  onSelect: () => void;
  danger?: boolean;
}

export interface DropdownProps {
  trigger: ReactNode;
  items: DropdownItem[];
  align?: "start" | "end";
}

export function Dropdown({ trigger, items, align = "start" }: DropdownProps) {
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const onClickOutside = (e: MouseEvent) => {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) setOpen(false);
    };
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") setOpen(false);
    };
    document.addEventListener("mousedown", onClickOutside);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("mousedown", onClickOutside);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [open]);

  return (
    <div ref={rootRef} className="relative inline-block">
      <button type="button" aria-haspopup="menu" aria-expanded={open} onClick={() => setOpen((v) => !v)}>
        {trigger}
      </button>
      {open && (
        <div
          role="menu"
          className={cn(
            "absolute top-full z-20 mt-2 min-w-40 rounded-[var(--radius-md)] border border-[var(--color-ink-200)] bg-[var(--color-ink-0)] p-1 shadow-[var(--shadow-raised)]",
            align === "end" ? "end-0" : "start-0"
          )}
        >
          {items.map((item) => (
            <button
              key={item.key}
              role="menuitem"
              onClick={() => {
                item.onSelect();
                setOpen(false);
              }}
              className={cn(
                "block w-full rounded-[var(--radius-sm)] px-3 py-2 text-start text-[var(--text-body-sm)] hover:bg-[var(--color-ink-100)]",
                item.danger ? "text-[var(--color-danger-500)]" : "text-[var(--color-ink-800)]"
              )}
            >
              {item.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
