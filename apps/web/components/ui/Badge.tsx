import type { HTMLAttributes } from "react";
import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "@/lib/utils/cn";

const badgeVariants = cva(
  "inline-flex items-center gap-1 rounded-[var(--radius-full)] px-2.5 py-1 text-[var(--text-caption)] font-medium",
  {
    variants: {
      variant: {
        neutral: "bg-[var(--color-ink-100)] text-[var(--color-ink-700)]",
        brand: "bg-[var(--color-brand-100)] text-[var(--color-brand-700)]",
        accent: "bg-[var(--color-accent-100)] text-[var(--color-accent-700)]",
        success: "bg-[var(--color-success-100)] text-[var(--color-success-500)]",
        warning: "bg-[var(--color-warning-100)] text-[var(--color-warning-500)]",
        danger: "bg-[var(--color-danger-100)] text-[var(--color-danger-500)]",
      },
    },
    defaultVariants: { variant: "neutral" },
  }
);

export interface BadgeProps extends HTMLAttributes<HTMLSpanElement>, VariantProps<typeof badgeVariants> {}

export function Badge({ className, variant, ...props }: BadgeProps) {
  return <span className={cn(badgeVariants({ variant }), className)} {...props} />;
}
