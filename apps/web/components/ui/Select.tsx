import { forwardRef, useId } from "react";
import type { SelectHTMLAttributes } from "react";
import { ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils/cn";

export interface SelectOption {
  value: string;
  label: string;
}

export interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  errorText?: string;
  options: SelectOption[];
  placeholder?: string;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ className, label, errorText, options, placeholder, id, ...props }, ref) => {
    const autoId = useId();
    const selectId = id ?? autoId;

    return (
      <div className="flex flex-col gap-1.5">
        {label && (
          <label htmlFor={selectId} className="text-[var(--text-body-sm)] font-medium text-[var(--color-ink-800)]">
            {label}
          </label>
        )}
        <div className="relative">
          <select
            ref={ref}
            id={selectId}
            aria-invalid={!!errorText || undefined}
            className={cn(
              "h-11 w-full appearance-none rounded-[var(--radius-md)] border bg-[var(--color-ink-0)] px-3 pe-9 text-[var(--text-body)] text-[var(--color-ink-900)] transition-colors",
              "border-[var(--color-ink-300)] focus:border-[var(--color-brand-500)]",
              errorText && "border-[var(--color-danger-500)]",
              className
            )}
            {...props}
          >
            {placeholder && (
              <option value="" disabled hidden>
                {placeholder}
              </option>
            )}
            {options.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
          <ChevronDown
            className="pointer-events-none absolute end-3 top-1/2 size-4 -translate-y-1/2 text-[var(--color-ink-500)]"
            aria-hidden="true"
          />
        </div>
        {errorText && (
          <p className="text-[var(--text-caption)] text-[var(--color-danger-500)]">{errorText}</p>
        )}
      </div>
    );
  }
);
Select.displayName = "Select";
