import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { Input } from "@/components/ui/Input";

describe("Input", () => {
  it("associates the label with the input via htmlFor/id", () => {
    render(<Input label="شماره موبایل" />);
    const input = screen.getByLabelText("شماره موبایل");
    expect(input).toBeInTheDocument();
  });

  it("lets the user type a value", async () => {
    render(<Input label="نام" />);
    const input = screen.getByLabelText("نام") as HTMLInputElement;
    await userEvent.type(input, "سارا");
    expect(input.value).toBe("سارا");
  });

  it("shows the error message and marks aria-invalid when errorText is set", () => {
    render(<Input label="ایمیل" errorText="ایمیل معتبر نیست" />);
    const input = screen.getByLabelText("ایمیل");
    expect(input).toHaveAttribute("aria-invalid", "true");
    expect(screen.getByText("ایمیل معتبر نیست")).toBeInTheDocument();
  });
});
