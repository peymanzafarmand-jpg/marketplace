import type { Metadata } from "next";
import localFont from "next/font/local";
import "@/styles/globals.css";

// Self-hosted (not fetched from Google Fonts at build/runtime) — the
// variable font covers weights 100–900, so every weight used across the
// design system comes from this single file. See README for licensing.
const vazirmatn = localFont({
  src: "./fonts/Vazirmatn-Variable.woff2",
  variable: "--font-vazirmatn",
  display: "swap",
  weight: "100 900",
});

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: {
    default: "مارکت کودک و نوزاد",
    template: "%s | مارکت کودک و نوزاد",
  },
  description: "مارکت‌پلیس تخصصی محصولات نوزاد، کودک و مادر — اصل، تضمین‌شده و با ارسال سریع.",
  openGraph: {
    type: "website",
    locale: "fa_IR",
    siteName: "مارکت کودک و نوزاد",
    title: "مارکت کودک و نوزاد",
    description: "مارکت‌پلیس تخصصی محصولات نوزاد، کودک و مادر.",
  },
  robots: {
    index: true,
    follow: true,
  },
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="fa" dir="rtl" className={vazirmatn.variable}>
      <body>{children}</body>
    </html>
  );
}
