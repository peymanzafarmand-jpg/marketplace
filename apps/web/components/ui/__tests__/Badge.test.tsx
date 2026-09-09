import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { Badge } from "@/components/ui/Badge";

describe("Badge", () => {
  it("renders its children text", () => {
    render(<Badge>۲۰٪ تخفیف</Badge>);
    expect(screen.getByText("۲۰٪ تخفیف")).toBeInTheDocument();
  });

  it("applies the variant class for the given variant", () => {
    render(<Badge variant="danger">ناموجود</Badge>);
    const badge = screen.getByText("ناموجود");
    expect(badge.className).toContain("danger");
  });
});
