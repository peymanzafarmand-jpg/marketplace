import { Skeleton } from "@/components/ui/Skeleton";
import { Container } from "@/components/layout/Container";

export default function Loading() {
  return (
    <Container className="flex flex-col gap-4 py-8">
      <Skeleton className="h-8 w-1/3" />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 6 }).map((_, i) => (
          <div key={i} className="flex flex-col gap-2">
            <Skeleton className="aspect-square w-full" />
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-4 w-1/2" />
          </div>
        ))}
      </div>
    </Container>
  );
}
