"use client";

import Link from "next/link";
import { Search, ShoppingBag, User, Menu } from "lucide-react";
import { Container } from "@/components/layout/Container";
import { DesktopNav } from "@/components/layout/DesktopNav";
import { useTranslations } from "@/hooks/use-translations";
import { useCartStore } from "@/lib/store/cart-store";
import { useUiStore } from "@/lib/store/ui-store";

export function Header() {
  const t = useTranslations();
  const itemCount = useCartStore((s) => s.itemCount);
  const openMobileMenu = useUiStore((s) => s.openMobileMenu);

  return (
    <header className="sticky top-0 z-40 border-b border-[var(--color-ink-200)] bg-[var(--color-ink-0)]">
      <Container className="flex h-16 items-center gap-4">
        <button
          type="button"
          onClick={openMobileMenu}
          aria-label="باز کردن منو"
          className="grid size-9 place-items-center rounded-[var(--radius-md)] text-[var(--color-ink-700)] hover:bg-[var(--color-ink-100)] md:hidden"
        >
          <Menu className="size-5" aria-hidden="true" />
        </button>

        <Link href="/" className="shrink-0 text-[var(--text-h3)] font-extrabold text-[var(--color-brand-600)]">
          {t.common.appName}
        </Link>

        <DesktopNav />

        <div className="relative ms-auto hidden max-w-sm flex-1 sm:block">
          <Search className="pointer-events-none absolute start-3 top-1/2 size-4 -translate-y-1/2 text-[var(--color-ink-400)]" aria-hidden="true" />
          <input
            type="search"
            placeholder={t.common.searchPlaceholder}
            aria-label={t.common.search}
            className="h-10 w-full rounded-[var(--radius-full)] border border-[var(--color-ink-300)] bg-[var(--color-ink-50)] ps-9 pe-4 text-[var(--text-body-sm)] focus:border-[var(--color-brand-500)]"
          />
        </div>

        <div className="ms-auto flex items-center gap-1 sm:ms-0">
          <Link
            href="/account"
            aria-label={t.common.account}
            className="hidden size-9 place-items-center rounded-[var(--radius-md)] text-[var(--color-ink-700)] hover:bg-[var(--color-ink-100)] sm:grid"
          >
            <User className="size-5" aria-hidden="true" />
          </Link>
          <Link
            href="/cart"
            aria-label={`${t.common.cart}${itemCount ? ` (${itemCount} کالا)` : ""}`}
            className="relative grid size-9 place-items-center rounded-[var(--radius-md)] text-[var(--color-ink-700)] hover:bg-[var(--color-ink-100)]"
          >
            <ShoppingBag className="size-5" aria-hidden="true" />
            {itemCount > 0 && (
              <span className="absolute -top-0.5 -end-0.5 grid min-w-4 place-items-center rounded-full bg-[var(--color-accent-500)] px-1 text-[10px] font-bold leading-4 text-white">
                {itemCount}
              </span>
            )}
          </Link>
        </div>
      </Container>
    </header>
  );
}
