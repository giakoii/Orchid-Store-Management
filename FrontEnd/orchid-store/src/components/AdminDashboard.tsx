'use client';

import React, { useState, useEffect } from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { API_CONFIG, buildApiUrl } from '@/config/api';

interface AdminStats {
  totalOrders: number;
  totalRevenue: number;
  pendingOrders: number;
  completedOrders: number;
}

interface BestSellingOrchid {
  orchidId: number;
  orchidName: string;
  totalSold: number;
  revenue: number;
}

interface AdminOrder {
  orderId: number;
  customerEmail: string;
  orderDate: string;
  orderStatus: string;
  totalAmount: number;
}

export default function AdminDashboard() {
  const { accessToken } = useAuth();
  const [activeTab, setActiveTab] = useState<'overview' | 'orchids' | 'categories' | 'orders'>('overview');
  const [stats, setStats] = useState<AdminStats | null>(null);
  const [bestSelling, setBestSelling] = useState<BestSellingOrchid[]>([]);
  const [orders, setOrders] = useState<AdminOrder[]>([]);
  const [loading, setLoading] = useState(true);

  // Orchid form states
  const [orchidForm, setOrchidForm] = useState({
    orchidName: '',
    orchidDescription: '',
    price: '',
    categoryId: '',
    isNatural: false,
    orchidImage: null as File | null
  });

  // Category form states
  const [categoryForm, setCategoryForm] = useState({
    categoryName: '',
    parentCategoryId: ''
  });

  useEffect(() => {
    if (accessToken) {
      fetchAdminData();
    }
  }, [accessToken]);

  const fetchAdminData = async () => {
    try {
      setLoading(true);

      // Fetch statistics
      const statsResponse = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.ADMIN_STATISTICS), {
        headers: { Authorization: `Bearer ${accessToken}` }
      });

      if (statsResponse.ok) {
        const statsData = await statsResponse.json();
        if (statsData.success) {
          setStats(statsData.response);
        }
      }

      // Fetch best selling orchids
      const bestSellingResponse = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.ADMIN_BEST_SELLING, { TopCount: 5 }), {
        headers: { Authorization: `Bearer ${accessToken}` }
      });

      if (bestSellingResponse.ok) {
        const bestSellingData = await bestSellingResponse.json();
        if (bestSellingData.success && Array.isArray(bestSellingData.response)) {
          setBestSelling(bestSellingData.response);
        } else {
          setBestSelling([]);
        }
      }

      // Fetch admin orders
      const ordersResponse = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.ADMIN_ORDERS, { PageNumber: 1, PageSize: 10 }), {
        headers: { Authorization: `Bearer ${accessToken}` }
      });

      if (ordersResponse.ok) {
        const ordersData = await ordersResponse.json();
        if (ordersData.success && Array.isArray(ordersData.response?.items)) {
          setOrders(ordersData.response.items);
        } else if (ordersData.success && Array.isArray(ordersData.response)) {
          setOrders(ordersData.response);
        } else {
          setOrders([]);
        }
      }

    } catch (error) {
      console.error('Error fetching admin data:', error);
      // Reset to empty arrays on error
      setBestSelling([]);
      setOrders([]);
    } finally {
      setLoading(false);
    }
  };

  const handleInsertOrchid = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!orchidForm.orchidImage) {
      alert('Vui lòng chọn hình ảnh hoa lan');
      return;
    }

    try {
      const formData = new FormData();
      formData.append('OrchidImage', orchidForm.orchidImage);

      const params = new URLSearchParams({
        OrchidName: orchidForm.orchidName,
        OrchidDescription: orchidForm.orchidDescription,
        Price: orchidForm.price,
        CategoryId: orchidForm.categoryId,
        IsNatural: orchidForm.isNatural.toString()
      });

      const response = await fetch(`${buildApiUrl(API_CONFIG.ENDPOINTS.INSERT_ORCHID)}?${params}`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${accessToken}` },
        body: formData
      });

      const result = await response.json();

      if (result.success) {
        alert('Thêm hoa lan thành công!');
        setOrchidForm({
          orchidName: '',
          orchidDescription: '',
          price: '',
          categoryId: '',
          isNatural: false,
          orchidImage: null
        });
      } else {
        alert(`Lỗi: ${result.message}`);
      }
    } catch (error) {
      console.error('Error inserting orchid:', error);
      alert('Có lỗi xảy ra khi thêm hoa lan');
    }
  };

  const handleInsertCategory = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      const response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.INSERT_CATEGORY), {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`
        },
        body: JSON.stringify({
          categoryName: categoryForm.categoryName,
          parentCategoryId: categoryForm.parentCategoryId ? parseInt(categoryForm.parentCategoryId) : null
        })
      });

      const result = await response.json();

      if (result.success) {
        alert('Thêm danh mục thành công!');
        setCategoryForm({ categoryName: '', parentCategoryId: '' });
      } else {
        alert(`Lỗi: ${result.message}`);
      }
    } catch (error) {
      console.error('Error inserting category:', error);
      alert('Có lỗi xảy ra khi thêm danh mục');
    }
  };

  const handleDeleteOrchid = async (orchidId: number) => {
    if (!confirm('Bạn có chắc chắn muốn xóa hoa lan này?')) return;

    try {
      const response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.DELETE_ORCHID), {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`
        },
        body: JSON.stringify({ orchidId })
      });

      const result = await response.json();

      if (result.success) {
        alert('Xóa hoa lan thành công!');
        // Refresh data
        fetchAdminData();
      } else {
        alert(`Lỗi: ${result.message}`);
      }
    } catch (error) {
      console.error('Error deleting orchid:', error);
      alert('Có lỗi xảy ra khi xóa hoa lan');
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-green-500"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Admin Dashboard</h1>
          <p className="text-gray-600">Quản lý cửa hàng hoa lan</p>
        </div>

        {/* Navigation Tabs */}
        <div className="mb-8">
          <div className="border-b border-gray-200">
            <nav className="-mb-px flex space-x-8">
              {[
                { id: 'overview', label: 'Tổng quan' },
                { id: 'orchids', label: 'Quản lý hoa lan' },
                { id: 'categories', label: 'Quản lý danh mục' },
                { id: 'orders', label: 'Quản lý đơn hàng' }
              ].map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id as any)}
                  className={`py-2 px-1 border-b-2 font-medium text-sm ${
                    activeTab === tab.id
                      ? 'border-green-500 text-green-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  {tab.label}
                </button>
              ))}
            </nav>
          </div>
        </div>

        {/* Content */}
        {activeTab === 'overview' && (
          <div className="space-y-8">
            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
              <div className="bg-white p-6 rounded-lg shadow">
                <h3 className="text-sm font-medium text-gray-500">Tổng đơn hàng</h3>
                <p className="text-3xl font-bold text-gray-900">{stats?.totalOrders || 0}</p>
              </div>
              <div className="bg-white p-6 rounded-lg shadow">
                <h3 className="text-sm font-medium text-gray-500">Doanh thu</h3>
                <p className="text-3xl font-bold text-green-600">
                  {stats?.totalRevenue?.toLocaleString('vi-VN')}₫
                </p>
              </div>
              <div className="bg-white p-6 rounded-lg shadow">
                <h3 className="text-sm font-medium text-gray-500">Đơn chờ xử lý</h3>
                <p className="text-3xl font-bold text-yellow-600">{stats?.pendingOrders || 0}</p>
              </div>
              <div className="bg-white p-6 rounded-lg shadow">
                <h3 className="text-sm font-medium text-gray-500">Đơn hoàn thành</h3>
                <p className="text-3xl font-bold text-green-600">{stats?.completedOrders || 0}</p>
              </div>
            </div>

            {/* Best Selling Orchids */}
            <div className="bg-white p-6 rounded-lg shadow">
              <h3 className="text-lg font-medium text-gray-900 mb-4">Hoa lan bán chạy nhất</h3>
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Tên hoa lan</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Đã bán</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Doanh thu</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {Array.isArray(bestSelling) && bestSelling.length > 0 ? (
                      bestSelling.map((item) => (
                        <tr key={item.orchidId}>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{item.orchidName}</td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{item.totalSold}</td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-green-600">
                            {item.revenue?.toLocaleString('vi-VN')}₫
                          </td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan={3} className="px-6 py-4 text-center text-sm text-gray-500">
                          Chưa có dữ liệu hoa lan bán chạy
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'orchids' && (
          <div className="space-y-8">
            {/* Add Orchid Form */}
            <div className="bg-white p-6 rounded-lg shadow">
              <h3 className="text-lg font-medium text-gray-900 mb-4">Thêm hoa lan mới</h3>
              <form onSubmit={handleInsertOrchid} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Tên hoa lan</label>
                    <input
                      type="text"
                      required
                      value={orchidForm.orchidName}
                      onChange={(e) => setOrchidForm({...orchidForm, orchidName: e.target.value})}
                      className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-green-500 focus:ring-green-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Giá (VNĐ)</label>
                    <input
                      type="number"
                      required
                      min="0.01"
                      step="0.01"
                      value={orchidForm.price}
                      onChange={(e) => setOrchidForm({...orchidForm, price: e.target.value})}
                      className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-green-500 focus:ring-green-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Danh mục ID</label>
                    <input
                      type="number"
                      required
                      value={orchidForm.categoryId}
                      onChange={(e) => setOrchidForm({...orchidForm, categoryId: e.target.value})}
                      className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-green-500 focus:ring-green-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Hình ảnh</label>
                    <input
                      type="file"
                      required
                      accept="image/*"
                      onChange={(e) => setOrchidForm({...orchidForm, orchidImage: e.target.files?.[0] || null})}
                      className="mt-1 block w-full"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Mô tả</label>
                  <textarea
                    required
                    rows={3}
                    value={orchidForm.orchidDescription}
                    onChange={(e) => setOrchidForm({...orchidForm, orchidDescription: e.target.value})}
                    className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-green-500 focus:ring-green-500"
                  />
                </div>
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    checked={orchidForm.isNatural}
                    onChange={(e) => setOrchidForm({...orchidForm, isNatural: e.target.checked})}
                    className="h-4 w-4 text-green-600 focus:ring-green-500 border-gray-300 rounded"
                  />
                  <label className="ml-2 block text-sm text-gray-900">Hoa lan tự nhiên</label>
                </div>
                <button
                  type="submit"
                  className="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500"
                >
                  Thêm hoa lan
                </button>
              </form>
            </div>
          </div>
        )}

        {activeTab === 'categories' && (
          <div className="space-y-8">
            {/* Add Category Form */}
            <div className="bg-white p-6 rounded-lg shadow">
              <h3 className="text-lg font-medium text-gray-900 mb-4">Thêm danh mục mới</h3>
              <form onSubmit={handleInsertCategory} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Tên danh mục</label>
                    <input
                      type="text"
                      required
                      value={categoryForm.categoryName}
                      onChange={(e) => setCategoryForm({...categoryForm, categoryName: e.target.value})}
                      className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-green-500 focus:ring-green-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Danh mục cha (tùy chọn)</label>
                    <input
                      type="number"
                      value={categoryForm.parentCategoryId}
                      onChange={(e) => setCategoryForm({...categoryForm, parentCategoryId: e.target.value})}
                      className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-green-500 focus:ring-green-500"
                    />
                  </div>
                </div>
                <button
                  type="submit"
                  className="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500"
                >
                  Thêm danh mục
                </button>
              </form>
            </div>
          </div>
        )}

        {activeTab === 'orders' && (
          <div className="bg-white p-6 rounded-lg shadow">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Đơn hàng gần đây</h3>
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Mã đơn</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Email khách hàng</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Ngày đặt</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Trạng thái</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Tổng tiền</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {Array.isArray(orders) && orders.length > 0 ? (
                    orders.map((order) => (
                      <tr key={order.orderId}>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">#{order.orderId}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{order.customerEmail}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                          {new Date(order.orderDate).toLocaleDateString('vi-VN')}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className={`px-2 py-1 text-xs rounded-full ${
                            order.orderStatus === 'Completed' ? 'bg-green-100 text-green-800' :
                            order.orderStatus === 'Pending' ? 'bg-yellow-100 text-yellow-800' :
                            'bg-gray-100 text-gray-800'
                          }`}>
                            {order.orderStatus}
                          </span>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-green-600">
                          {order.totalAmount?.toLocaleString('vi-VN')}₫
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={5} className="px-6 py-4 text-center text-sm text-gray-500">
                        Chưa có đơn hàng nào
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
