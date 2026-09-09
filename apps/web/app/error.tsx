"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/Button";
import { Container } from "@/components/layout/Container";
import { useTranslations } from "@/hooks/use-translations";

export default function Error({ error, reset }: { error: Error & { digest?: string }; reset: () => void }) {
  const t = useTranslations();
  const router = useRouter();

  useEffect(() => {
    // Foundation-level logging hook. Wire to a real monitoring service
    // (Sentry, etc.) when one is added to the project.
    console.error(error);
  }, [error]);

  return (
    <Container className="flex min-h-[60vh] flex-col items-center justify-center gap-4 py-12 text-center">
      <h1 className="text-[var(--text-h1)] font-extrabold text-[var(--color-ink-900)]">
        {t.errors.globalTitle}
      </h1>
      <p className="max-w-sm text-[var(--text-body)] text-[var(--color-ink-600)]">
        {t.errors.globalDescription}
      </p>
      <div className="flex gap-2">
        <Button onClick={reset}>{t.errors.retry}</Button>
        <Button variant="outline" onClick={() => router.push("/")}>
          {t.errors.backHome}
        </Button>
      </div>
    </Container>
  );
}
