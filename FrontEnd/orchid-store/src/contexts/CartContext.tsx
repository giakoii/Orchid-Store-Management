'use client';

import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
  useCallback,
  useMemo,
} from 'react';
import { Cart, CartItem, OrderRequest, ApiOrderResponse } from '@/types/cart';
import { Orchid } from '@/types/orchid';
import { API_CONFIG, buildApiUrl } from '@/config/api';
import { useAuth } from './AuthContext';

interface CartContextType {
  cart: Cart;
  addToCart: (orchid: Orchid, quantity?: number) => void;
  removeFromCart: (orchidId: number) => void;
  updateQuantity: (orchidId: number, quantity: number) => void;
  clearCart: () => void;
  createOrder: () => Promise<{ success: boolean; data?: ApiOrderResponse; error?: string }>;
  isCreatingOrder: boolean;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

const STORAGE_KEY = 'orchid_cart';

export function CartProvider({ children }: Readonly<{ children: ReactNode }>) {
  const { accessToken } = useAuth();
  const [cart, setCart] = useState<Cart>({
    items: [],
    totalItems: 0,
    totalAmount: 0,
  });
  const [isCreatingOrder, setIsCreatingOrder] = useState(false);

  // Load cart from localStorage on mount
  useEffect(() => {
    const savedCart = localStorage.getItem(STORAGE_KEY);
    if (savedCart) {
      try {
        const parsedCart = JSON.parse(savedCart);
        setCart(parsedCart);
      } catch (error) {
        console.error('Error loading cart from localStorage:', error);
      }
    }
  }, []);

  // Save cart to localStorage whenever cart changes
  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(cart));
  }, [cart]);

  // Calculate totals
  const calculateTotals = useCallback((items: CartItem[]) => {
    const totalItems = items.reduce((sum, item) => sum + item.quantity, 0);
    const totalAmount = items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    return { totalItems, totalAmount };
  }, []);

  // Add item to cart
  const addToCart = useCallback((orchid: Orchid, quantity: number = 1) => {
    setCart(prevCart => {
      const existingItemIndex = prevCart.items.findIndex(item => item.orchidId === orchid.orchidId);
      let newItems: CartItem[];

      if (existingItemIndex >= 0) {
        // Update existing item
        newItems = [...prevCart.items];
        newItems[existingItemIndex].quantity += quantity;
      } else {
        // Add new item
        const newItem: CartItem = {
          orchidId: orchid.orchidId,
          orchidName: orchid.orchidName,
          price: orchid.price,
          quantity,
          imageUrl: orchid.orchidUrl,
        };
        newItems = [...prevCart.items, newItem];
      }

      const { totalItems, totalAmount } = calculateTotals(newItems);
      return {
        items: newItems,
        totalItems,
        totalAmount,
      };
    });
  }, [calculateTotals]);

  // Remove item from cart
  const removeFromCart = useCallback((orchidId: number) => {
    setCart(prevCart => {
      const newItems = prevCart.items.filter(item => item.orchidId !== orchidId);
      const { totalItems, totalAmount } = calculateTotals(newItems);
      return {
        items: newItems,
        totalItems,
        totalAmount,
      };
    });
  }, [calculateTotals]);

  // Update item quantity
  const updateQuantity = useCallback((orchidId: number, quantity: number) => {
    if (quantity <= 0) {
      removeFromCart(orchidId);
      return;
    }

    setCart(prevCart => {
      const newItems = prevCart.items.map(item =>
        item.orchidId === orchidId ? { ...item, quantity } : item
      );
      const { totalItems, totalAmount } = calculateTotals(newItems);
      return {
        items: newItems,
        totalItems,
        totalAmount,
      };
    });
  }, [calculateTotals, removeFromCart]);

  // Clear cart
  const clearCart = useCallback(() => {
    setCart({
      items: [],
      totalItems: 0,
      totalAmount: 0,
    });
  }, []);

  // Create order
  const createOrder = useCallback(async (): Promise<{ success: boolean; data?: ApiOrderResponse; error?: string }> => {
    if (!accessToken) {
      return { success: false, error: 'Bạn cần đăng nhập để tạo đơn hàng' };
    }

    if (cart.items.length === 0) {
      return { success: false, error: 'Giỏ hàng trống' };
    }

    setIsCreatingOrder(true);

    try {
      const orderRequest: OrderRequest = {
        items: cart.items.map(item => ({
          orchidId: item.orchidId,
          quantity: item.quantity,
        })),
      };

      const response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.INSERT_ORDER), {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`,
          'accept': '*/*',
        },
        body: JSON.stringify(orderRequest),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data: ApiOrderResponse = await response.json();

      if (data.success) {
        // Clear cart after successful order creation
        clearCart();
        return { success: true, data };
      } else {
        return { success: false, error: data.message || 'Tạo đơn hàng thất bại' };
      }
    } catch (error) {
      console.error('Error creating order:', error);
      return { success: false, error: 'Có lỗi xảy ra khi tạo đơn hàng' };
    } finally {
      setIsCreatingOrder(false);
    }
  }, [accessToken, cart.items, clearCart]);

  return (
    <CartContext.Provider
      value={useMemo(() => ({
        cart,
        addToCart,
        removeFromCart,
        updateQuantity,
        clearCart,
        createOrder,
        isCreatingOrder,
      }), [cart, addToCart, removeFromCart, updateQuantity, clearCart, createOrder, isCreatingOrder])}
    >
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  const context = useContext(CartContext);
  if (context === undefined) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
}
