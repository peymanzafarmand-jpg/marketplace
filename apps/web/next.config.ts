import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Produces a minimal, self-contained server build in .next/standalone —
  // this is what the Dockerfile copies into the runtime image.
  output: "standalone",

  images: {
    // Add real CDN/storage hostnames here once the backend is wired up.
    remotePatterns: [],
  },
};

export default nextConfig;
