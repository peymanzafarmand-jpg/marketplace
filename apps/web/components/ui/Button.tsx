import { forwardRef } from "react";
import type { ButtonHTMLAttributes } from "react";
import { cva, type VariantProps } from "class-variance-authority";
import { Loader2 } from "lucide-react";
import { cn } from "@/lib/utils/cn";

export const buttonVariants = cva(
  "inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-[var(--radius-md)] font-medium transition-colors disabled:pointer-events-none disabled:opacity-50",
  {
    variants: {
      variant: {
        primary: "bg-[var(--color-brand-500)] text-white hover:bg-[var(--color-brand-600)]",
        secondary: "bg-[var(--color-accent-500)] text-white hover:bg-[var(--color-accent-600)]",
        outline:
          "border border-[var(--color-ink-300)] bg-transparent text-[var(--color-ink-900)] hover:bg-[var(--color-ink-100)]",
        ghost: "bg-transparent text-[var(--color-ink-900)] hover:bg-[var(--color-ink-100)]",
        danger: "bg-[var(--color-danger-500)] text-white hover:opacity-90",
      },
      size: {
        sm: "h-9 px-3 text-[var(--text-body-sm)]",
        md: "h-11 px-4 text-[var(--text-body)]",
        lg: "h-13 px-6 text-[var(--text-h3)]",
        icon: "h-11 w-11 p-0",
      },
    },
    defaultVariants: { variant: "primary", size: "md" },
  }
);

export interface ButtonProps
  extends ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  isLoading?: boolean;
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, isLoading, disabled, children, ...props }, ref) => {
    return (
      <button
        ref={ref}
        className={cn(buttonVariants({ variant, size }), className)}
        disabled={disabled || isLoading}
        aria-busy={isLoading || undefined}
        {...props}
      >
        {isLoading && <Loader2 className="size-4 animate-spin" aria-hidden="true" />}
        {children}
      </button>
    );
  }
);
Button.displayName = "Button";
