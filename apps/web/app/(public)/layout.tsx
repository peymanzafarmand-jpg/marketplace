import { PublicLayout } from "@/components/layout/PublicLayout";

export default function PublicRouteGroupLayout({ children }: { children: React.ReactNode }) {
  return <PublicLayout>{children}</PublicLayout>;
}
