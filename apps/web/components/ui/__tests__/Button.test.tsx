import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { Button } from "@/components/ui/Button";

describe("Button", () => {
  it("renders its label", () => {
    render(<Button>ثبت سفارش</Button>);
    expect(screen.getByRole("button", { name: "ثبت سفارش" })).toBeInTheDocument();
  });

  it("calls onClick when clicked", async () => {
    const onClick = vi.fn();
    render(<Button onClick={onClick}>افزودن به سبد</Button>);
    await userEvent.click(screen.getByRole("button", { name: "افزودن به سبد" }));
    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it("is disabled and does not fire onClick while isLoading", async () => {
    const onClick = vi.fn();
    render(
      <Button isLoading onClick={onClick}>
        در حال ارسال
      </Button>
    );
    const button = screen.getByRole("button", { name: "در حال ارسال" });
    expect(button).toBeDisabled();
    expect(button).toHaveAttribute("aria-busy", "true");
    await userEvent.click(button);
    expect(onClick).not.toHaveBeenCalled();
  });
});
