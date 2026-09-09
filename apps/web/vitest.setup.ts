import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { afterEach } from "vitest";

// Ensure DOM is reset between tests to avoid state leaking across cases.
afterEach(() => {
  cleanup();
});
