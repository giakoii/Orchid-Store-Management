export interface Orchid {
  orchidId: number;
  orchidName: string;
  orchidDescription: string;
  price: number;
  categoryId: number;
  categoryName?: string;
  isNatural: boolean;
  orchidUrl?: string;
  createdDate?: string;
  isDeleted?: boolean;
}

export interface Category {
  categoryId: number;
  categoryName: string;
  parentCategoryId?: number;
  parentCategoryName?: string;
  isDeleted?: boolean;
}

export interface AdminStats {
  totalOrders: number;
  totalRevenue: number;
  pendingOrders: number;
  completedOrders: number;
}

export interface BestSellingOrchid {
  orchidId: number;
  orchidName: string;
  categoryName?: string | null;
  price: number;
  totalQuantitySold: number;
  totalRevenue: number;
  orderCount: number;
  imageUrl?: string;
  // Legacy fields for backward compatibility
  totalSold?: number;
  revenue?: number;
}

export interface AdminOrder {
  orderId: number;
  customerEmail: string;
  orderDate: string;
  orderStatus: string;
  totalAmount: number;
  items?: AdminOrderItem[];
}

export interface AdminOrderItem {
  orchidId: number;
  orchidName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

// Order History Types
export interface OrderHistory {
  id: number;
  orderDate: string;
}

export interface OrderDetail {
  id: number;
  accountId: number;
  orderDate: string;
  orderStatus: string;
  totalAmount: number;
  orderDetails: OrderItem[];
}

export interface OrderItem {
  orchidId: number;
  orchidName: string;
  quantity: number;
  price: number;
  totalPrice?: number;
}

export interface OrchidFormData {
  orchidName: string;
  orchidDescription: string;
  price: string;
  categoryId: string;
  isNatural: boolean;
  orchidImage: File | null;
}

export interface CategoryFormData {
  categoryName: string;
  parentCategoryId: string;
}

// API Response Types
export interface OrchidResponse {
  success: boolean;
  response: {
    orchids: Orchid[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
  };
}

export interface CategoryResponse {
  success: boolean;
  response: {
    categories: Category[];
  };
}
