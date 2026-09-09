import Link from "next/link";
import { buttonVariants } from "@/components/ui/Button";
import { Container } from "@/components/layout/Container";
import fa from "@/lib/i18n/dictionaries/fa";

export default function NotFound() {
  const t = fa;

  return (
    <Container className="flex min-h-[60vh] flex-col items-center justify-center gap-4 py-12 text-center">
      <p className="text-[var(--text-display)] font-extrabold text-[var(--color-brand-500)]">۴۰۴</p>
      <h1 className="text-[var(--text-h1)] font-extrabold text-[var(--color-ink-900)]">
        {t.errors.notFoundTitle}
      </h1>
      <p className="max-w-sm text-[var(--text-body)] text-[var(--color-ink-600)]">
        {t.errors.notFoundDescription}
      </p>
      <Link href="/" className={buttonVariants({ variant: "primary", size: "md" })}>
        {t.errors.backHome}
      </Link>
    </Container>
  );
}
