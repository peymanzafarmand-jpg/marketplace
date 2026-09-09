import { describe, it, expect, vi } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";

/**
 * Placeholder test-infrastructure example. Replace the inline component
 * with an import of the real `LoginForm` once implementation begins
 * (Auth UI feature sprint).
 *
 * Uses phone number, not email, as the primary login identifier — this
 * matches the actual Identity data model (apps/api/src/Core/Marketplace.Domain
 * /Modules/Identity/User.cs) and the market this product is built for, where
 * phone-based login is the norm. An earlier version of this placeholder used
 * an email field; fixed during Task 1.7 (Repo Integration & Verification) so
 * it doesn't mislead whoever builds the real form.
 *
 * Demonstrates the expected pattern: accessible queries, no reliance on
 * implementation detail, and a guard against leaking password values into
 * the DOM as plain readable text (basic front-end security regression).
 */
function LoginForm({ onSubmit }: { onSubmit: (phone: string, password: string) => void }) {
  return (
    <form
      onSubmit={(e) => {
        e.preventDefault();
        const form = e.currentTarget;
        const phone = (form.elements.namedItem("phone") as HTMLInputElement).value;
        const password = (form.elements.namedItem("password") as HTMLInputElement).value;
        onSubmit(phone, password);
      }}
    >
      <label htmlFor="phone">شماره موبایل</label>
      <input id="phone" name="phone" type="tel" inputMode="numeric" autoComplete="tel" required />

      <label htmlFor="password">رمز عبور</label>
      <input id="password" name="password" type="password" required />

      <button type="submit">ورود</button>
    </form>
  );
}

describe("LoginForm", () => {
  it("submits phone and password to the handler", () => {
    const handleSubmit = vi.fn();
    render(<LoginForm onSubmit={handleSubmit} />);

    fireEvent.change(screen.getByLabelText("شماره موبایل"), {
      target: { value: "09120000000" },
    });
    fireEvent.change(screen.getByLabelText("رمز عبور"), {
      target: { value: "Sup3rSecret!" },
    });
    fireEvent.click(screen.getByRole("button", { name: "ورود" }));

    expect(handleSubmit).toHaveBeenCalledWith("09120000000", "Sup3rSecret!");
  });

  it("uses type=tel for the phone field (correct mobile keyboard, not email)", () => {
    render(<LoginForm onSubmit={vi.fn()} />);
    expect(screen.getByLabelText("شماره موبایل")).toHaveAttribute("type", "tel");
  });

  it("renders the password field with type=password (never plain text)", () => {
    render(<LoginForm onSubmit={vi.fn()} />);
    const passwordInput = screen.getByLabelText("رمز عبور");
    expect(passwordInput).toHaveAttribute("type", "password");
  });
});
