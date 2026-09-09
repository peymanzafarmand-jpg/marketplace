"use client";

import Link from "next/link";
import { Drawer } from "@/components/ui/Drawer";
import { useUiStore } from "@/lib/store/ui-store";
import { useTranslations } from "@/hooks/use-translations";

const hrefs = ["/", "/categories", "/brands", "/deals", "/magazine"];

export function MobileMenuDrawer() {
  const t = useTranslations();
  const isOpen = useUiStore((s) => s.isMobileMenuOpen);
  const closeMobileMenu = useUiStore((s) => s.closeMobileMenu);

  return (
    <Drawer open={isOpen} onClose={closeMobileMenu} title={t.common.appName} side="end">
      <nav aria-label="ناوبری اصلی" className="flex flex-col gap-1">
        {t.nav.desktop.map((label, i) => (
          <Link
            key={label}
            href={hrefs[i] ?? "/"}
            onClick={closeMobileMenu}
            className="rounded-[var(--radius-md)] px-3 py-2.5 text-[var(--text-body)] text-[var(--color-ink-800)] hover:bg-[var(--color-ink-100)]"
          >
            {label}
          </Link>
        ))}
      </nav>
    </Drawer>
  );
}
