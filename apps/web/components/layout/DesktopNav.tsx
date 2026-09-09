"use client";

import Link from "next/link";
import { useTranslations } from "@/hooks/use-translations";

const hrefs = ["/", "/categories", "/brands", "/deals", "/magazine"];

export function DesktopNav() {
  const t = useTranslations();

  return (
    <nav aria-label="ناوبری اصلی" className="hidden items-center gap-6 md:flex">
      {t.nav.desktop.map((label, i) => (
        <Link
          key={label}
          href={hrefs[i] ?? "/"}
          className="text-[var(--text-body-sm)] font-medium text-[var(--color-ink-700)] transition-colors hover:text-[var(--color-brand-600)]"
        >
          {label}
        </Link>
      ))}
    </nav>
  );
}
