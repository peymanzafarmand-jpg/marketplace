export interface User {
  id: string;
  fullName: string;
  phone: string;
  email?: string;
  avatarUrl?: string;
}

export interface CartLineItem {
  id: string;
  productId: string;
  title: string;
  thumbnailUrl?: string;
  unitPrice: number;
  quantity: number;
  variant?: string;
}
