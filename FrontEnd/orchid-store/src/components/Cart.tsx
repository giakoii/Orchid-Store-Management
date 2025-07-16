'use client';

import React, { useState } from 'react';
import { useCart } from '@/contexts/CartContext';
import { useAuth } from '@/contexts/AuthContext';
import { formatCurrency } from '@/utils/format';

interface CartProps {
  readonly isOpen: boolean;
  readonly onCloseAction: () => void;
}

export default function Cart({ isOpen, onCloseAction }: CartProps) {
  const { cart, removeFromCart, updateQuantity, createOrder, isCreatingOrder } = useCart();
  const { isAuthenticated } = useAuth();
  const [orderResult, setOrderResult] = useState<string | null>(null);

  const handleQuantityChange = (orchidId: number, newQuantity: number) => {
    if (newQuantity < 1) return;
    updateQuantity(orchidId, newQuantity);
  };

  const handleCreateOrder = async () => {
    if (!isAuthenticated) {
      setOrderResult('Vui lòng đăng nhập để tạo đơn hàng');
      return;
    }

    const result = await createOrder();
    if (result.success && result.data) {
      // Redirect to payment URL
      if (result.data.response.paymentUrl) {
        window.open(result.data.response.paymentUrl, '_blank');
        setOrderResult('Đơn hàng đã được tạo thành công! Đang chuyển đến trang thanh toán...');
        setTimeout(() => {
          onCloseAction();
          setOrderResult(null);
        }, 2000);
      }
    } else {
      setOrderResult(result.error ?? 'Có lỗi xảy ra khi tạo đơn hàng');
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-hidden">
      {/* Backdrop with beautiful gradient background */}
      <div
        className="absolute inset-0 bg-gradient-to-br from-purple-900/80 via-pink-900/80 to-indigo-900/80 backdrop-blur-sm transition-opacity"
        style={{
          backgroundImage: `
            radial-gradient(circle at 20% 80%, rgba(120, 119, 198, 0.3) 0%, transparent 50%),
            radial-gradient(circle at 80% 20%, rgba(255, 119, 198, 0.3) 0%, transparent 50%),
            radial-gradient(circle at 40% 40%, rgba(120, 119, 198, 0.2) 0%, transparent 50%)
          `
        }}
        onClick={onCloseAction}
        onKeyDown={(e) => {
          if (e.key === 'Escape') {
            onCloseAction();
          }
        }}
        role="button"
        tabIndex={0}
        aria-label="Close cart"
      />

      {/* Cart Panel with enhanced styling */}
      <div className="absolute right-0 top-0 h-full w-full max-w-md bg-white/95 backdrop-blur-lg shadow-2xl border-l border-purple-200">
        <div className="flex h-full flex-col">
          {/* Header with gradient */}
          <div className="flex items-center justify-between bg-gradient-to-r from-purple-600 to-pink-600 text-white px-6 py-4 shadow-lg">
            <h2 className="text-xl font-bold">
              🛍️ Giỏ hàng ({cart.totalItems})
            </h2>
            <button
              onClick={onCloseAction}
              className="rounded-full p-2 text-white/80 hover:text-white hover:bg-white/20 transition-all duration-200"
            >
              <span className="sr-only">Đóng</span>
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Cart Items */}
          <div className="flex-1 overflow-y-auto px-4 py-4">
            {cart.items.length === 0 ? (
              <div className="flex h-full items-center justify-center">
                <div className="text-center">
                  <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                  </svg>
                  <p className="mt-2 text-sm text-gray-500">Giỏ hàng trống</p>
                </div>
              </div>
            ) : (
              <div className="space-y-4">
                {cart.items.map((item) => (
                  <div key={item.orchidId} className="flex items-center space-x-4 rounded-lg border p-4">
                    {/* Product Image */}
                    <div className="h-16 w-16 flex-shrink-0 overflow-hidden rounded-md">
                      {item.imageUrl ? (
                        <img
                          src={item.imageUrl}
                          alt={item.orchidName}
                          className="h-full w-full object-cover object-center"
                        />
                      ) : (
                        <div className="flex h-full w-full items-center justify-center bg-gray-200">
                          <svg className="h-8 w-8 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                          </svg>
                        </div>
                      )}
                    </div>

                    {/* Product Details */}
                    <div className="flex-1">
                      <h3 className="text-sm font-medium text-gray-900">{item.orchidName}</h3>
                      <p className="text-sm text-gray-500">{formatCurrency(item.price)}</p>

                      {/* Quantity Controls */}
                      <div className="mt-2 flex items-center space-x-2">
                        <button
                          onClick={() => handleQuantityChange(item.orchidId, item.quantity - 1)}
                          className="rounded-md bg-gray-100 p-1 text-gray-600 hover:bg-gray-200"
                          disabled={item.quantity <= 1}
                        >
                          <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 12H4" />
                          </svg>
                        </button>
                        <span className="min-w-[2rem] text-center text-sm font-medium">
                          {item.quantity}
                        </span>
                        <button
                          onClick={() => handleQuantityChange(item.orchidId, item.quantity + 1)}
                          className="rounded-md bg-gray-100 p-1 text-gray-600 hover:bg-gray-200"
                        >
                          <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                          </svg>
                        </button>
                      </div>
                    </div>

                    {/* Item Total & Remove */}
                    <div className="flex flex-col items-end space-y-2">
                      <p className="text-sm font-medium text-gray-900">
                        {formatCurrency(item.price * item.quantity)}
                      </p>
                      <button
                        onClick={() => removeFromCart(item.orchidId)}
                        className="text-red-600 hover:text-red-500"
                      >
                        <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                        </svg>
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Cart Summary & Checkout */}
          {cart.items.length > 0 && (
            <div className="border-t px-4 py-4">
              {/* Total */}
              <div className="flex justify-between text-base font-medium text-gray-900">
                <p>Tổng cộng</p>
                <p>{formatCurrency(cart.totalAmount)}</p>
              </div>
              <p className="mt-0.5 text-sm text-gray-500">
                Phí vận chuyển sẽ được tính khi thanh toán
              </p>

              {/* Order Result Message */}
              {orderResult && (
                <div className={`mt-4 rounded-md p-3 text-sm ${
                  orderResult.includes('thành công') ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                }`}>
                  {orderResult}
                </div>
              )}

              {/* Checkout Button */}
              <div className="mt-6">
                <button
                  onClick={handleCreateOrder}
                  disabled={isCreatingOrder || !isAuthenticated}
                  className="w-full rounded-md bg-indigo-600 px-6 py-3 text-base font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:bg-gray-400 disabled:cursor-not-allowed"
                >
                  {(() => {
                    if (isCreatingOrder) {
                      return (
                        <div className="flex items-center justify-center">
                          <svg className="mr-2 h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24">
                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                            <path className="opacity-75" fill="currentColor" d="m4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                          </svg>
                          Đang tạo đơn hàng...
                        </div>
                      );
                    }
                    if (!isAuthenticated) {
                      return 'Đăng nhập để đặt hàng';
                    }
                    return 'Đặt hàng';
                  })()}
                </button>
              </div>

              {/* Continue Shopping */}
              <div className="mt-4">
                <button
                  onClick={onCloseAction}
                  className="w-full rounded-md border border-gray-300 bg-white px-6 py-3 text-base font-medium text-gray-700 shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
                >
                  Tiếp tục mua sắm
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
