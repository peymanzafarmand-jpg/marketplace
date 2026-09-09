import Link from "next/link";
import { Container } from "@/components/layout/Container";
import { useTranslations } from "@/hooks/use-translations";

export function Footer() {
  const t = useTranslations();
  const year = new Date().getFullYear();

  return (
    <footer className="mt-auto border-t border-[var(--color-ink-200)] bg-[var(--color-ink-0)]">
      <Container className="flex flex-col gap-4 py-8 sm:flex-row sm:items-center sm:justify-between">
        <p className="text-[var(--text-body-sm)] font-bold text-[var(--color-brand-600)]">
          {t.common.appName}
        </p>
        <nav aria-label="لینک‌های فوتر" className="flex flex-wrap gap-4 text-[var(--text-body-sm)] text-[var(--color-ink-600)]">
          <Link href="/about" className="hover:text-[var(--color-brand-600)]">{t.footer.about}</Link>
          <Link href="/support" className="hover:text-[var(--color-brand-600)]">{t.footer.support}</Link>
          <Link href="/terms" className="hover:text-[var(--color-brand-600)]">{t.footer.terms}</Link>
        </nav>
        <p className="text-[var(--text-caption)] text-[var(--color-ink-400)]">
          © {year} {t.common.appName} — {t.footer.rights}
        </p>
      </Container>
    </footer>
  );
}
