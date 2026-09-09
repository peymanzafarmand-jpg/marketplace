import { create } from "zustand";

export type ToastVariant = "success" | "error" | "warning" | "info";

export interface ToastMessage {
  id: string;
  variant: ToastVariant;
  title: string;
  description?: string;
}

interface UiState {
  isMobileMenuOpen: boolean;
  isCartDrawerOpen: boolean;
  toasts: ToastMessage[];
  openMobileMenu: () => void;
  closeMobileMenu: () => void;
  openCartDrawer: () => void;
  closeCartDrawer: () => void;
  pushToast: (toast: Omit<ToastMessage, "id">) => void;
  dismissToast: (id: string) => void;
}

/**
 * Global, ephemeral UI state that many unrelated components need to
 * read/trigger (nav drawer, cart drawer, toast queue). Anything scoped
 * to a single component tree should stay as local useState instead.
 */
export const useUiStore = create<UiState>()((set) => ({
  isMobileMenuOpen: false,
  isCartDrawerOpen: false,
  toasts: [],
  openMobileMenu: () => set({ isMobileMenuOpen: true }),
  closeMobileMenu: () => set({ isMobileMenuOpen: false }),
  openCartDrawer: () => set({ isCartDrawerOpen: true }),
  closeCartDrawer: () => set({ isCartDrawerOpen: false }),
  pushToast: (toast) =>
    set((state) => ({
      toasts: [...state.toasts, { ...toast, id: crypto.randomUUID() }],
    })),
  dismissToast: (id) =>
    set((state) => ({ toasts: state.toasts.filter((t) => t.id !== id) })),
}));
