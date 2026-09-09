import { forwardRef, useId } from "react";
import type { InputHTMLAttributes } from "react";
import { Check } from "lucide-react";
import { cn } from "@/lib/utils/cn";

export interface CheckboxProps extends Omit<InputHTMLAttributes<HTMLInputElement>, "type"> {
  label?: string;
}

export const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(
  ({ className, label, id, ...props }, ref) => {
    const autoId = useId();
    const checkboxId = id ?? autoId;

    return (
      <label htmlFor={checkboxId} className="inline-flex items-center gap-2 cursor-pointer select-none">
        <span className="relative inline-flex">
          <input
            ref={ref}
            type="checkbox"
            id={checkboxId}
            className={cn("peer size-5 appearance-none rounded-[var(--radius-sm)] border border-[var(--color-ink-300)] bg-[var(--color-ink-0)] checked:border-[var(--color-brand-500)] checked:bg-[var(--color-brand-500)] transition-colors", className)}
            {...props}
          />
          <Check
            className="pointer-events-none absolute inset-0 m-auto size-3.5 text-white opacity-0 peer-checked:opacity-100"
            aria-hidden="true"
          />
        </span>
        {label && <span className="text-[var(--text-body-sm)] text-[var(--color-ink-800)]">{label}</span>}
      </label>
    );
  }
);
Checkbox.displayName = "Checkbox";
