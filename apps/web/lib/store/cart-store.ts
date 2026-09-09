import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { CartLineItem } from "@/types/domain";

interface CartState {
  items: CartLineItem[];
  itemCount: number;
  addItem: (item: CartLineItem) => void;
  removeItem: (itemId: string) => void;
  setQuantity: (itemId: string, quantity: number) => void;
  clear: () => void;
}

/**
 * Foundation only. Real behavior (merging duplicate variants, syncing
 * with the server cart for logged-in users, price recalculation) is
 * out of scope for this task and will land with the cart feature.
 */
export const useCartStore = create<CartState>()(
  persist(
    (set, get) => ({
      items: [],
      itemCount: 0,
      addItem: (item) => {
        const items = [...get().items, item];
        set({ items, itemCount: items.reduce((n, i) => n + i.quantity, 0) });
      },
      removeItem: (itemId) => {
        const items = get().items.filter((i) => i.id !== itemId);
        set({ items, itemCount: items.reduce((n, i) => n + i.quantity, 0) });
      },
      setQuantity: (itemId, quantity) => {
        const items = get().items.map((i) => (i.id === itemId ? { ...i, quantity } : i));
        set({ items, itemCount: items.reduce((n, i) => n + i.quantity, 0) });
      },
      clear: () => set({ items: [], itemCount: 0 }),
    }),
    { name: "cart-store" }
  )
);
