// Cart types
export interface CartItem {
  orchidId: number;
  orchidName: string;
  price: number;
  quantity: number;
  imageUrl?: string;
}

export interface Cart {
  items: CartItem[];
  totalItems: number;
  totalAmount: number;
}

export interface OrderRequest {
  items: {
    orchidId: number;
    quantity: number;
  }[];
}

export interface OrderResponse {
  paymentUrl: string;
  qrCodeUrl: string;
  deeplink: string;
  deeplinkWebInApp: string;
}

export interface ApiOrderResponse {
  response: OrderResponse;
  success: boolean;
  messageId: string;
  message: string;
  detailErrorList: any;
}
