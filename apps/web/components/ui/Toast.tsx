"use client";

import { useEffect } from "react";
import { CheckCircle2, AlertCircle, AlertTriangle, Info, X } from "lucide-react";
import { cn } from "@/lib/utils/cn";
import { useUiStore, type ToastMessage, type ToastVariant } from "@/lib/store/ui-store";

const variantConfig: Record<ToastVariant, { icon: typeof Info; classes: string }> = {
  success: { icon: CheckCircle2, classes: "border-[var(--color-success-500)] text-[var(--color-success-500)]" },
  error: { icon: AlertCircle, classes: "border-[var(--color-danger-500)] text-[var(--color-danger-500)]" },
  warning: { icon: AlertTriangle, classes: "border-[var(--color-warning-500)] text-[var(--color-warning-500)]" },
  info: { icon: Info, classes: "border-[var(--color-info-500)] text-[var(--color-info-500)]" },
};

function ToastItem({ toast }: { toast: ToastMessage }) {
  const dismissToast = useUiStore((s) => s.dismissToast);
  const { icon: Icon, classes } = variantConfig[toast.variant];

  useEffect(() => {
    const timer = setTimeout(() => dismissToast(toast.id), 5000);
    return () => clearTimeout(timer);
  }, [toast.id, dismissToast]);

  return (
    <div
      role="status"
      className={cn(
        "flex w-full items-start gap-3 rounded-[var(--radius-md)] border bg-[var(--color-ink-0)] p-3 shadow-[var(--shadow-raised)]",
        classes
      )}
    >
      <Icon className="mt-0.5 size-5 shrink-0" aria-hidden="true" />
      <div className="flex-1">
        <p className="text-[var(--text-body-sm)] font-medium text-[var(--color-ink-900)]">{toast.title}</p>
        {toast.description && (
          <p className="mt-0.5 text-[var(--text-caption)] text-[var(--color-ink-600)]">{toast.description}</p>
        )}
      </div>
      <button
        type="button"
        onClick={() => dismissToast(toast.id)}
        aria-label="بستن پیام"
        className="text-[var(--color-ink-400)] hover:text-[var(--color-ink-700)]"
      >
        <X className="size-4" aria-hidden="true" />
      </button>
    </div>
  );
}

/** Mount once near the root layout. Reads the global toast queue from ui-store. */
export function ToastContainer() {
  const toasts = useUiStore((s) => s.toasts);

  return (
    <div
      aria-live="polite"
      className="pointer-events-none fixed inset-x-0 top-4 z-[60] mx-auto flex w-full max-w-sm flex-col gap-2 px-4"
    >
      {toasts.map((toast) => (
        <div key={toast.id} className="pointer-events-auto">
          <ToastItem toast={toast} />
        </div>
      ))}
    </div>
  );
}
