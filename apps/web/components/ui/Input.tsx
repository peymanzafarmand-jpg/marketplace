import { forwardRef, useId } from "react";
import type { InputHTMLAttributes } from "react";
import { cn } from "@/lib/utils/cn";

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  helperText?: string;
  errorText?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ className, label, helperText, errorText, id, ...props }, ref) => {
    const autoId = useId();
    const inputId = id ?? autoId;
    const describedBy = errorText
      ? `${inputId}-error`
      : helperText
        ? `${inputId}-helper`
        : undefined;

    return (
      <div className="flex flex-col gap-1.5">
        {label && (
          <label htmlFor={inputId} className="text-[var(--text-body-sm)] font-medium text-[var(--color-ink-800)]">
            {label}
          </label>
        )}
        <input
          ref={ref}
          id={inputId}
          aria-invalid={!!errorText || undefined}
          aria-describedby={describedBy}
          className={cn(
            "h-11 w-full rounded-[var(--radius-md)] border bg-[var(--color-ink-0)] px-3 text-[var(--text-body)] text-[var(--color-ink-900)] placeholder:text-[var(--color-ink-400)] transition-colors",
            "border-[var(--color-ink-300)] focus:border-[var(--color-brand-500)]",
            errorText && "border-[var(--color-danger-500)] focus:border-[var(--color-danger-500)]",
            className
          )}
          {...props}
        />
        {errorText ? (
          <p id={`${inputId}-error`} className="text-[var(--text-caption)] text-[var(--color-danger-500)]">
            {errorText}
          </p>
        ) : helperText ? (
          <p id={`${inputId}-helper`} className="text-[var(--text-caption)] text-[var(--color-ink-500)]">
            {helperText}
          </p>
        ) : null}
      </div>
    );
  }
);
Input.displayName = "Input";
