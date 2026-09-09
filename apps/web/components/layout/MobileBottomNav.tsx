"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Home, Grid2x2, Search, ShoppingBag, User } from "lucide-react";
import { cn } from "@/lib/utils/cn";
import { useTranslations } from "@/hooks/use-translations";
import { useCartStore } from "@/lib/store/cart-store";

const icons = { home: Home, categories: Grid2x2, search: Search, cart: ShoppingBag, account: User };
const hrefs: Record<string, string> = {
  home: "/",
  categories: "/categories",
  search: "/search",
  cart: "/cart",
  account: "/account",
};

export function MobileBottomNav() {
  const t = useTranslations();
  const pathname = usePathname();
  const itemCount = useCartStore((s) => s.itemCount);

  return (
    <nav
      aria-label="ناوبری پایین صفحه"
      className="fixed inset-x-0 bottom-0 z-40 border-t border-[var(--color-ink-200)] bg-[var(--color-ink-0)] pb-[env(safe-area-inset-bottom)] md:hidden"
    >
      <ul className="grid grid-cols-5">
        {t.nav.mobile.map(({ label, key }) => {
          const Icon = icons[key as keyof typeof icons];
          const href = hrefs[key];
          const isActive = pathname === href;
          return (
            <li key={key}>
              <Link
                href={href}
                aria-current={isActive ? "page" : undefined}
                className={cn(
                  "relative flex flex-col items-center gap-1 py-2 text-[10px] font-medium",
                  isActive ? "text-[var(--color-brand-600)]" : "text-[var(--color-ink-500)]"
                )}
              >
                <Icon className="size-5" aria-hidden="true" />
                {label}
                {key === "cart" && itemCount > 0 && (
                  <span className="absolute end-6 top-1 grid min-w-4 place-items-center rounded-full bg-[var(--color-accent-500)] px-1 text-[9px] font-bold leading-4 text-white">
                    {itemCount}
                  </span>
                )}
              </Link>
            </li>
          );
        })}
      </ul>
    </nav>
  );
}
