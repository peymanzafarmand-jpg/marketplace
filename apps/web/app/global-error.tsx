"use client";

import { useEffect } from "react";

/**
 * Catches errors thrown by the root layout itself (fonts, providers, etc.)
 * — the one place in Next.js where the boundary must render its own
 * <html>/<body>, since the root layout that would normally provide them
 * is what failed.
 */
export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <html lang="fa" dir="rtl">
      <body style={{ fontFamily: "Tahoma, sans-serif" }}>
        <div style={{ minHeight: "100vh", display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", gap: 16, textAlign: "center", padding: 24 }}>
          <h1 style={{ fontSize: 24, fontWeight: 800 }}>یک مشکل پیش آمد</h1>
          <p style={{ maxWidth: 360, color: "#5c564c" }}>
            برنامه با خطا مواجه شد. لطفاً صفحه را دوباره بارگذاری کنید.
          </p>
          <button
            onClick={reset}
            style={{ height: 44, padding: "0 20px", borderRadius: 8, background: "#2f8d82", color: "#fff", border: "none", fontWeight: 600 }}
          >
            تلاش دوباره
          </button>
        </div>
      </body>
    </html>
  );
}
