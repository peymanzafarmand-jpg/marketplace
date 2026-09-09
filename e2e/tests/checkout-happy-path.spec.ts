import { test, expect } from "@playwright/test";

/**
 * Placeholder E2E test-infrastructure example.
 * Once product/checkout pages exist, replace selectors below with real
 * `data-testid` attributes from the implementation.
 */
test.describe("Checkout — happy path", () => {
  test("guest can browse a product and reach the payment step", async ({ page }) => {
    await page.goto("/");

    // TODO: replace with real search/product selectors once implemented
    await expect(page).toHaveTitle(/Baby.*Marketplace/i);

    // await page.getByPlaceholder("Search products").fill("stroller");
    // await page.getByRole("button", { name: "Search" }).click();
    // await page.getByTestId("product-card").first().click();
    // await page.getByRole("button", { name: "Add to cart" }).click();
    // await page.getByRole("link", { name: "Checkout" }).click();
    // await expect(page.getByTestId("payment-step")).toBeVisible();
  });

  test("security headers are present on the home page response", async ({ page }) => {
    const response = await page.goto("/");
    expect(response).not.toBeNull();

    const headers = response!.headers();
    expect(headers["x-content-type-options"]).toBe("nosniff");
    expect(headers["x-frame-options"]).toBe("DENY");
    expect(headers["referrer-policy"]).toBe("strict-origin-when-cross-origin");
    expect(headers["content-security-policy"]).toBeTruthy();
  });
});
