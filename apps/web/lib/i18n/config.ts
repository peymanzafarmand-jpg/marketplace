/**
 * i18n foundation.
 *
 * The product is Persian-first and RTL-only for now — there is no locale
 * switcher yet and `app/layout.tsx` hardcodes `lang="fa"` / `dir="rtl"`.
 * This file exists so that adding a second locale later is a matter of:
 *   1. adding its entry to `locales` below,
 *   2. adding a dictionary file next to `fa.ts`,
 *   3. wiring a `[locale]` route segment and a locale switcher.
 * No component should import raw strings from here directly — use the
 * `useTranslations()` hook in `hooks/use-translations.ts`.
 */

export type Locale = "fa";

export const defaultLocale: Locale = "fa";

export const locales: Record<Locale, { label: string; dir: "rtl" | "ltr" }> = {
  fa: { label: "فارسی", dir: "rtl" },
};
