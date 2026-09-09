import type { HTMLAttributes } from "react";
import { cn } from "@/lib/utils/cn";

export function Skeleton({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      role="presentation"
      aria-hidden="true"
      className={cn("animate-pulse rounded-[var(--radius-md)] bg-[var(--color-ink-200)]", className)}
      {...props}
    />
  );
}
