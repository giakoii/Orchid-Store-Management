'use client';

import React, { useState, useEffect } from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { API_CONFIG, buildApiUrl } from '@/config/api';
import { OrderHistory, OrderDetail } from '@/types/orchid';
import { formatCurrency } from '@/utils/format';

interface OrderHistoryModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export default function OrderHistoryModal({ isOpen, onClose }: OrderHistoryModalProps) {
  const { accessToken } = useAuth();
  const [orders, setOrders] = useState<OrderHistory[]>([]);
  const [selectedOrder, setSelectedOrder] = useState<OrderDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const [orderDetailLoading, setOrderDetailLoading] = useState(false);

  useEffect(() => {
    if (isOpen && accessToken) {
      fetchOrders();
    }
  }, [isOpen, accessToken]);

  const fetchOrders = async () => {
    try {
      setLoading(true);
      const response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.SELECT_ORDERS), {
        headers: {
          'Authorization': `Bearer ${accessToken}`,
          'accept': '*/*'
        }
      });

      if (response.ok) {
        const data = await response.json();
        console.log('Orders response:', data);

        // Extract orders from response
        const ordersData = Array.isArray(data.response) ? data.response : [];
        setOrders(ordersData);
      } else {
        console.error('Failed to fetch orders:', response.statusText);
      }
    } catch (error) {
      console.error('Error fetching orders:', error);
    } finally {
      setLoading(false);
    }
  };

  const fetchOrderDetail = async (orderId: number) => {
    try {
      setOrderDetailLoading(true);
      const response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.SELECT_ORDER, { OrderId: orderId }), {
        headers: {
          'Authorization': `Bearer ${accessToken}`,
          'accept': '*/*'
        }
      });

      if (response.ok) {
        const data = await response.json();
        console.log('Order detail response:', data);

        if (data.response) {
          setSelectedOrder(data.response);
        }
      } else {
        console.error('Failed to fetch order detail:', response.statusText);
      }
    } catch (error) {
      console.error('Error fetching order detail:', error);
    } finally {
      setOrderDetailLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'completed':
        return 'bg-green-100 text-green-800';
      case 'processing':
        return 'bg-yellow-100 text-yellow-800';
      case 'pending':
        return 'bg-blue-100 text-blue-800';
      case 'cancelled':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div className="relative top-20 mx-auto p-5 border w-5/6 max-w-4xl shadow-lg rounded-md bg-white">
        <div className="mt-3">
          {/* Header */}
          <div className="flex justify-between items-center mb-6">
            <h3 className="text-lg font-medium text-gray-900">Lịch sử đơn hàng</h3>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Content */}
          <div className="flex gap-6">
            {/* Orders List */}
            <div className="w-1/2">
              <h4 className="font-medium text-gray-900 mb-4">Danh sách đơn hàng</h4>
              {loading ? (
                <div className="flex justify-center py-8">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-green-600"></div>
                </div>
              ) : orders.length === 0 ? (
                <p className="text-gray-500 text-center py-8">Không có đơn hàng nào</p>
              ) : (
                <div className="space-y-3 max-h-96 overflow-y-auto">
                  {orders.map((order) => (
                    <div
                      key={order.id}
                      onClick={() => fetchOrderDetail(order.id)}
                      className={`p-3 border rounded-lg cursor-pointer hover:bg-gray-50 transition-colors ${
                        selectedOrder?.id === order.id ? 'border-green-500 bg-green-50' : 'border-gray-200'
                      }`}
                    >
                      <div className="flex justify-between items-center">
                        <div>
                          <p className="font-medium text-gray-900">Đơn hàng #{order.id}</p>
                          <p className="text-sm text-gray-500">{formatDate(order.orderDate)}</p>
                        </div>
                        <svg className="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 5l7 7-7 7" />
                        </svg>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* Order Detail */}
            <div className="w-1/2 border-l pl-6">
              <h4 className="font-medium text-gray-900 mb-4">Chi tiết đơn hàng</h4>
              {orderDetailLoading ? (
                <div className="flex justify-center py-8">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-green-600"></div>
                </div>
              ) : selectedOrder ? (
                <div className="space-y-4">
                  {/* Order Info */}
                  <div className="bg-gray-50 p-4 rounded-lg">
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <p className="text-gray-500">Mã đơn hàng:</p>
                        <p className="font-medium">#{selectedOrder.id}</p>
                      </div>
                      <div>
                        <p className="text-gray-500">Ngày đặt:</p>
                        <p className="font-medium">{formatDate(selectedOrder.orderDate)}</p>
                      </div>
                      <div>
                        <p className="text-gray-500">Trạng thái:</p>
                        <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(selectedOrder.orderStatus)}`}>
                          {selectedOrder.orderStatus}
                        </span>
                      </div>
                      <div>
                        <p className="text-gray-500">Tổng tiền:</p>
                        <p className="font-medium text-lg">{formatCurrency(selectedOrder.totalAmount)}</p>
                      </div>
                    </div>
                  </div>

                  {/* Order Items */}
                  <div>
                    <h5 className="font-medium text-gray-900 mb-3">Sản phẩm đã đặt</h5>
                    <div className="space-y-3">
                      {selectedOrder.orderDetails.map((item, index) => (
                        <div key={index} className="flex justify-between items-center p-3 bg-white border rounded-lg">
                          <div>
                            <p className="font-medium text-gray-900">{item.orchidName}</p>
                            <p className="text-sm text-gray-500">Số lượng: {item.quantity}</p>
                          </div>
                          <div className="text-right">
                            <p className="font-medium">{formatCurrency(item.price)}</p>
                            <p className="text-sm text-gray-500">x{item.quantity}</p>
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              ) : (
                <p className="text-gray-500 text-center py-8">Chọn đơn hàng để xem chi tiết</p>
              )}
            </div>
          </div>

          {/* Footer */}
          <div className="flex justify-end mt-6 pt-4 border-t">
            <button
              onClick={onClose}
              className="bg-gray-300 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-400 transition-colors"
            >
              Đóng
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
