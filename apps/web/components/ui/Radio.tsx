import { forwardRef, useId } from "react";
import type { InputHTMLAttributes } from "react";
import { cn } from "@/lib/utils/cn";

export interface RadioProps extends Omit<InputHTMLAttributes<HTMLInputElement>, "type"> {
  label?: string;
}

export const Radio = forwardRef<HTMLInputElement, RadioProps>(
  ({ className, label, id, ...props }, ref) => {
    const autoId = useId();
    const radioId = id ?? autoId;

    return (
      <label htmlFor={radioId} className="inline-flex items-center gap-2 cursor-pointer select-none">
        <span className="relative inline-flex">
          <input
            ref={ref}
            type="radio"
            id={radioId}
            className={cn(
              "peer size-5 appearance-none rounded-full border border-[var(--color-ink-300)] bg-[var(--color-ink-0)] checked:border-[5px] checked:border-[var(--color-brand-500)] transition-all",
              className
            )}
            {...props}
          />
        </span>
        {label && <span className="text-[var(--text-body-sm)] text-[var(--color-ink-800)]">{label}</span>}
      </label>
    );
  }
);
Radio.displayName = "Radio";
