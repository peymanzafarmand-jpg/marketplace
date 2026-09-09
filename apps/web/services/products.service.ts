import { apiClient } from "@/services/api-client";

/**
 * Example service — shows the pattern feature services should follow.
 * Not wired to any UI yet; the marketplace catalog endpoints will be
 * added in the features phase.
 */
export interface ProductSummary {
  id: string;
  title: string;
  price: number;
  thumbnailUrl: string;
}

export const productsService = {
  list: () => apiClient.get<ProductSummary[]>("/products"),
  getById: (id: string) => apiClient.get<ProductSummary>(`/products/${id}`),
};
