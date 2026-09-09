import type { ReactNode } from "react";
import { Header } from "@/components/layout/Header";
import { Footer } from "@/components/layout/Footer";
import { MobileBottomNav } from "@/components/layout/MobileBottomNav";
import { MobileMenuDrawer } from "@/components/layout/MobileMenuDrawer";
import { ToastContainer } from "@/components/ui/Toast";

export function PublicLayout({ children }: { children: ReactNode }) {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="flex-1 pb-bottom-nav md:pb-0">{children}</main>
      <Footer />
      <MobileBottomNav />
      <MobileMenuDrawer />
      <ToastContainer />
    </div>
  );
}
