import { forwardRef, useId } from "react";
import type { TextareaHTMLAttributes } from "react";
import { cn } from "@/lib/utils/cn";

export interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
  helperText?: string;
  errorText?: string;
}

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ className, label, helperText, errorText, id, rows = 4, ...props }, ref) => {
    const autoId = useId();
    const textareaId = id ?? autoId;

    return (
      <div className="flex flex-col gap-1.5">
        {label && (
          <label htmlFor={textareaId} className="text-[var(--text-body-sm)] font-medium text-[var(--color-ink-800)]">
            {label}
          </label>
        )}
        <textarea
          ref={ref}
          id={textareaId}
          rows={rows}
          aria-invalid={!!errorText || undefined}
          className={cn(
            "w-full resize-y rounded-[var(--radius-md)] border bg-[var(--color-ink-0)] px-3 py-2 text-[var(--text-body)] text-[var(--color-ink-900)] placeholder:text-[var(--color-ink-400)] transition-colors",
            "border-[var(--color-ink-300)] focus:border-[var(--color-brand-500)]",
            errorText && "border-[var(--color-danger-500)] focus:border-[var(--color-danger-500)]",
            className
          )}
          {...props}
        />
        {errorText ? (
          <p className="text-[var(--text-caption)] text-[var(--color-danger-500)]">{errorText}</p>
        ) : helperText ? (
          <p className="text-[var(--text-caption)] text-[var(--color-ink-500)]">{helperText}</p>
        ) : null}
      </div>
    );
  }
);
Textarea.displayName = "Textarea";
