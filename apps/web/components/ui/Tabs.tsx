"use client";

import { useId, useState } from "react";
import type { ReactNode } from "react";
import { cn } from "@/lib/utils/cn";

export interface TabItem {
  key: string;
  label: string;
  content: ReactNode;
}

export interface TabsProps {
  items: TabItem[];
  defaultKey?: string;
  className?: string;
}

export function Tabs({ items, defaultKey, className }: TabsProps) {
  const [activeKey, setActiveKey] = useState(defaultKey ?? items[0]?.key);
  const baseId = useId();

  return (
    <div className={className}>
      <div role="tablist" aria-label="برگه‌ها" className="flex gap-1 border-b border-[var(--color-ink-200)]">
        {items.map((item) => {
          const isActive = item.key === activeKey;
          return (
            <button
              key={item.key}
              role="tab"
              id={`${baseId}-tab-${item.key}`}
              aria-selected={isActive}
              aria-controls={`${baseId}-panel-${item.key}`}
              onClick={() => setActiveKey(item.key)}
              className={cn(
                "-mb-px border-b-2 px-4 py-2.5 text-[var(--text-body-sm)] font-medium transition-colors",
                isActive
                  ? "border-[var(--color-brand-500)] text-[var(--color-brand-600)]"
                  : "border-transparent text-[var(--color-ink-500)] hover:text-[var(--color-ink-800)]"
              )}
            >
              {item.label}
            </button>
          );
        })}
      </div>
      {items.map((item) => (
        <div
          key={item.key}
          role="tabpanel"
          id={`${baseId}-panel-${item.key}`}
          aria-labelledby={`${baseId}-tab-${item.key}`}
          hidden={item.key !== activeKey}
          className="py-4"
        >
          {item.content}
        </div>
      ))}
    </div>
  );
}
