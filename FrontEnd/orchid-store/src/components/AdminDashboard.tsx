'use client';

import React, { useState, useEffect } from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { API_CONFIG, buildApiUrl } from '@/config/api';
import { Orchid, Category, AdminStats, BestSellingOrchid, AdminOrder, OrchidFormData, CategoryFormData } from '@/types/orchid';

export default function AdminDashboard() {
  const { accessToken } = useAuth();
  const [activeTab, setActiveTab] = useState<'overview' | 'orchids' | 'categories' | 'orders'>('overview');
  const [stats, setStats] = useState<AdminStats | null>(null);
  const [bestSelling, setBestSelling] = useState<BestSellingOrchid[]>([]);
  const [orders, setOrders] = useState<AdminOrder[]>([]);
  const [orchids, setOrchids] = useState<Orchid[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [showAddModal, setShowAddModal] = useState(false);
  const [editingItem, setEditingItem] = useState<any>(null);

  // Pagination states
  const [orchidPagination, setOrchidPagination] = useState({
    currentPage: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0
  });

  const [categoryPagination, setCategoryPagination] = useState({
    currentPage: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0
  });

  // Form states
  const [orchidForm, setOrchidForm] = useState<OrchidFormData>({
    orchidName: '',
    orchidDescription: '',
    price: '',
    categoryId: '',
    isNatural: false,
    orchidImage: null
  });

  const [categoryForm, setCategoryForm] = useState<CategoryFormData>({
    categoryName: '',
    parentCategoryId: ''
  });

  // Fetch admin statistics
  const fetchStats = async () => {
    try {
      const response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.ADMIN_STATISTICS), {
        headers: { 'Authorization': `Bearer ${accessToken}` }
      });
      if (response.ok) {
        const data = await response.json();
        setStats(data.response);
      }
    } catch (error) {
      console.error('Error fetching stats:', error);
    }
  };

  // Fetch best selling orchids
  const fetchBestSelling = async () => {
    try {
      const response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.ADMIN_BEST_SELLING), {
        headers: { 'Authorization': `Bearer ${accessToken}` }
      });
      if (response.ok) {
        const data = await response.json();
        console.log('Best Selling API Response:', data);

        // Fix: Extract best selling data from the correct path
        const bestSellingData = Array.isArray(data.response?.bestSellingOrchids) ? data.response.bestSellingOrchids :
                              Array.isArray(data.response?.items) ? data.response.items :
                              Array.isArray(data.response?.bestSelling) ? data.response.bestSelling :
                              Array.isArray(data.response) ? data.response :
                              Array.isArray(data.items) ? data.items :
                              Array.isArray(data) ? data : [];

        console.log('Extracted best selling data:', bestSellingData);
        setBestSelling(bestSellingData);
      }
    } catch (error) {
      console.error('Error fetching best selling:', error);
      setBestSelling([]);
    }
  };

  // Fetch admin orders
  const fetchOrders = async () => {
    try {
      const response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.ADMIN_ORDERS), {
        headers: { 'Authorization': `Bearer ${accessToken}` }
      });
      if (response.ok) {
        const data = await response.json();
        // Fix: Extract orders data properly
        const ordersData = Array.isArray(data.response?.items) ? data.response.items :
                          Array.isArray(data.response?.orders) ? data.response.orders :
                          Array.isArray(data.response) ? data.response :
                          Array.isArray(data.items) ? data.items :
                          Array.isArray(data) ? data : [];
        setOrders(ordersData);
      }
    } catch (error) {
      console.error('Error fetching orders:', error);
      setOrders([]);
    }
  };

  // Fetch orchids - FIXED with pagination
  const fetchOrchids = async (page: number = orchidPagination.currentPage) => {
    try {
      console.log('Fetching orchids...');
      const url = buildApiUrl(API_CONFIG.ENDPOINTS.SELECT_ORCHIDS) +
        `?PageNumber=${page}&PageSize=${orchidPagination.pageSize}`;

      const response = await fetch(url, {
        headers: { 'Authorization': `Bearer ${accessToken}` }
      });
      console.log('Orchids response status:', response.status);

      if (response.ok) {
        const data = await response.json();
        console.log('Orchids response data:', data);

        // Fix: Extract orchids from the correct path based on actual API response structure
        let orchidsData = [];
        let totalCount = 0;
        let pageNumber = 1;
        let pageSize = 10;

        if (data.response && Array.isArray(data.response.orchids)) {
          orchidsData = data.response.orchids;
          totalCount = data.response.totalCount || orchidsData.length;
          pageNumber = data.response.pageNumber || 1;
          pageSize = data.response.pageSize || 10;
          console.log('Using data.response.orchids, length:', orchidsData.length);
        } else if (data.response && Array.isArray(data.response.items)) {
          orchidsData = data.response.items;
          totalCount = data.response.totalCount || orchidsData.length;
          pageNumber = data.response.pageNumber || 1;
          pageSize = data.response.pageSize || 10;
          console.log('Using data.response.items');
        } else if (data.response && Array.isArray(data.response)) {
          orchidsData = data.response;
          totalCount = orchidsData.length;
          console.log('Using data.response');
        } else if (Array.isArray(data.items)) {
          orchidsData = data.items;
          totalCount = data.totalCount || orchidsData.length;
          pageNumber = data.pageNumber || 1;
          pageSize = data.pageSize || 10;
          console.log('Using data.items');
        } else if (Array.isArray(data)) {
          orchidsData = data;
          totalCount = orchidsData.length;
          console.log('Using data directly');
        } else {
          console.log('No array found, using empty array');
          orchidsData = [];
        }

        console.log('Final orchidsData:', orchidsData);
        console.log('Final orchidsData length:', orchidsData.length);

        setOrchids(orchidsData);
        setOrchidPagination({
          currentPage: pageNumber,
          pageSize: pageSize,
          totalCount: totalCount,
          totalPages: Math.ceil(totalCount / pageSize)
        });
      } else {
        console.log('Failed to fetch orchids, setting empty array');
        setOrchids([]);
      }
    } catch (error) {
      console.error('Error fetching orchids:', error);
      setOrchids([]);
    }
  };

  // Fetch categories - FIXED with pagination
  const fetchCategories = async (page: number = categoryPagination.currentPage) => {
    try {
      console.log('Fetching categories...');
      const url = buildApiUrl(API_CONFIG.ENDPOINTS.SELECT_CATEGORIES) +
        `?PageNumber=${page}&PageSize=${categoryPagination.pageSize}`;

      const response = await fetch(url, {
        headers: { 'Authorization': `Bearer ${accessToken}` }
      });
      console.log('Categories response status:', response.status);

      if (response.ok) {
        const data = await response.json();
        console.log('Categories response data:', data);

        // Fix: Extract categories from the correct path based on actual API response structure
        let categoriesData = [];
        let totalCount = 0;
        let pageNumber = 1;
        let pageSize = 10;

        if (data.response && Array.isArray(data.response.categories)) {
          categoriesData = data.response.categories;
          totalCount = data.response.totalCount || categoriesData.length;
          pageNumber = data.response.pageNumber || 1;
          pageSize = data.response.pageSize || 10;
          console.log('Using data.response.categories, length:', categoriesData.length);
        } else if (data.response && Array.isArray(data.response.items)) {
          categoriesData = data.response.items;
          totalCount = data.response.totalCount || categoriesData.length;
          pageNumber = data.response.pageNumber || 1;
          pageSize = data.response.pageSize || 10;
          console.log('Using data.response.items');
        } else if (data.response && Array.isArray(data.response)) {
          categoriesData = data.response;
          totalCount = categoriesData.length;
          console.log('Using data.response');
        } else if (Array.isArray(data.items)) {
          categoriesData = data.items;
          totalCount = data.totalCount || categoriesData.length;
          pageNumber = data.pageNumber || 1;
          pageSize = data.pageSize || 10;
          console.log('Using data.items');
        } else if (Array.isArray(data)) {
          categoriesData = data;
          totalCount = categoriesData.length;
          console.log('Using data directly');
        } else {
          console.log('No array found, using empty array');
          categoriesData = [];
        }

        console.log('Final categoriesData:', categoriesData);
        console.log('Final categoriesData length:', categoriesData.length);

        setCategories(categoriesData);
        setCategoryPagination({
          currentPage: pageNumber,
          pageSize: pageSize,
          totalCount: totalCount,
          totalPages: Math.ceil(totalCount / pageSize)
        });
      } else {
        console.log('Failed to fetch categories, setting empty array');
        setCategories([]);
      }
    } catch (error) {
      console.error('Error fetching categories:', error);
      setCategories([]);
    }
  };

  // Helper function to get category name by ID
  const getCategoryName = (categoryId: number) => {
    const category = categories.find(cat => cat.categoryId === categoryId);
    return category ? category.categoryName : `Category ${categoryId}`;
  };

  // Helper function to get parent category name by ID
  const getParentCategoryName = (parentCategoryId: number | null | undefined) => {
    if (!parentCategoryId) return 'None';
    const category = categories.find(cat => cat.categoryId === parentCategoryId);
    return category ? category.categoryName : `Category ${parentCategoryId}`;
  };

  // Pagination handlers
  const handleOrchidPageChange = (page: number) => {
    setOrchidPagination(prev => ({ ...prev, currentPage: page }));
    fetchOrchids(page);
  };

  const handleCategoryPageChange = (page: number) => {
    setCategoryPagination(prev => ({ ...prev, currentPage: page }));
    fetchCategories(page);
  };

  // Pagination component
  const PaginationComponent = ({
    currentPage,
    totalPages,
    onPageChange
  }: {
    currentPage: number;
    totalPages: number;
    onPageChange: (page: number) => void;
  }) => {
    if (totalPages <= 1) return null;

    const pages = [];
    const maxVisiblePages = 5;

    let startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
    let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

    if (endPage - startPage + 1 < maxVisiblePages) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return (
      <div className="flex items-center justify-between border-t border-gray-200 bg-white px-4 py-3 sm:px-6">
        <div className="flex flex-1 justify-between sm:hidden">
          <button
            onClick={() => onPageChange(Math.max(1, currentPage - 1))}
            disabled={currentPage === 1}
            className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
          >
            Previous
          </button>
          <button
            onClick={() => onPageChange(Math.min(totalPages, currentPage + 1))}
            disabled={currentPage === totalPages}
            className="relative ml-3 inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
          >
            Next
          </button>
        </div>
        <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
          <div>
            <p className="text-sm text-gray-700">
              Showing page <span className="font-medium">{currentPage}</span> of{' '}
              <span className="font-medium">{totalPages}</span>
            </p>
          </div>
          <div>
            <nav className="isolate inline-flex -space-x-px rounded-md shadow-sm" aria-label="Pagination">
              <button
                onClick={() => onPageChange(Math.max(1, currentPage - 1))}
                disabled={currentPage === 1}
                className="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0 disabled:opacity-50"
              >
                <span className="sr-only">Previous</span>
                <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                  <path fillRule="evenodd" d="M12.79 5.23a.75.75 0 01-.02 1.06L8.832 10l3.938 3.71a.75.75 0 11-1.04 1.08l-4.5-4.25a.75.75 0 010-1.08l4.5-4.25a.75.75 0 011.06.02z" clipRule="evenodd" />
                </svg>
              </button>

              {startPage > 1 && (
                <>
                  <button
                    onClick={() => onPageChange(1)}
                    className="relative inline-flex items-center px-4 py-2 text-sm font-semibold text-gray-900 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0"
                  >
                    1
                  </button>
                  {startPage > 2 && (
                    <span className="relative inline-flex items-center px-4 py-2 text-sm font-semibold text-gray-700 ring-1 ring-inset ring-gray-300 focus:outline-offset-0">
                      ...
                    </span>
                  )}
                </>
              )}

              {pages.map((page) => (
                <button
                  key={page}
                  onClick={() => onPageChange(page)}
                  className={`relative inline-flex items-center px-4 py-2 text-sm font-semibold ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0 ${
                    page === currentPage
                      ? 'z-10 bg-green-600 text-white focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-green-600'
                      : 'text-gray-900'
                  }`}
                >
                  {page}
                </button>
              ))}

              {endPage < totalPages && (
                <>
                  {endPage < totalPages - 1 && (
                    <span className="relative inline-flex items-center px-4 py-2 text-sm font-semibold text-gray-700 ring-1 ring-inset ring-gray-300 focus:outline-offset-0">
                      ...
                    </span>
                  )}
                  <button
                    onClick={() => onPageChange(totalPages)}
                    className="relative inline-flex items-center px-4 py-2 text-sm font-semibold text-gray-900 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0"
                  >
                    {totalPages}
                  </button>
                </>
              )}

              <button
                onClick={() => onPageChange(Math.min(totalPages, currentPage + 1))}
                disabled={currentPage === totalPages}
                className="relative inline-flex items-center rounded-r-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0 disabled:opacity-50"
              >
                <span className="sr-only">Next</span>
                <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                  <path fillRule="evenodd" d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z" clipRule="evenodd" />
                </svg>
              </button>
            </nav>
          </div>
        </div>
      </div>
    );
  };

  // Delete orchid
  const deleteOrchid = async (orchidId: number) => {
    try {
      const response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.DELETE_ORCHID), {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`
        },
        body: JSON.stringify({ orchidId })
      });

      if (response.ok) {
        alert('Orchid deleted successfully!');
        fetchOrchids(); // Refresh list
      } else {
        alert('Failed to delete orchid');
      }
    } catch (error) {
      console.error('Error deleting orchid:', error);
      alert('Error deleting orchid');
    }
  };

  // Delete category
  const deleteCategory = async (categoryId: number) => {
    try {
      const response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.DELETE_CATEGORY), {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`
        },
        body: JSON.stringify({ categoryId })
      });

      const data = await response.json();

      if (response.ok && data.success) {
        alert(data.message || 'Category deleted successfully!');
        fetchCategories(); // Refresh list
      } else {
        // Hiển thị message từ API khi thất bại
        const errorMessage = data.message || 'Failed to delete category';
        alert(errorMessage);
      }
    } catch (error) {
      console.error('Error deleting category:', error);
      alert('Error deleting category');
    }
  };

  // Add/Update orchid
  const handleOrchidSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      const formData = new FormData();
      if (orchidForm.orchidImage) {
        formData.append('OrchidImage', orchidForm.orchidImage);
      }

      const isUpdate = editingItem !== null;
      const endpoint = isUpdate ? API_CONFIG.ENDPOINTS.UPDATE_ORCHID : API_CONFIG.ENDPOINTS.INSERT_ORCHID;

      let url = buildApiUrl(endpoint);
      const params = new URLSearchParams();

      if (isUpdate) {
        params.append('OrchidId', editingItem.orchidId.toString());
      }
      if (orchidForm.orchidName) params.append('OrchidName', orchidForm.orchidName);
      if (orchidForm.orchidDescription) params.append('OrchidDescription', orchidForm.orchidDescription);
      if (orchidForm.price) params.append('Price', orchidForm.price);
      if (orchidForm.categoryId) params.append('CategoryId', orchidForm.categoryId);
      params.append('IsNatural', orchidForm.isNatural.toString());

      url += '?' + params.toString();

      const response = await fetch(url, {
        method: isUpdate ? 'PUT' : 'POST',
        headers: {
          'Authorization': `Bearer ${accessToken}`
        },
        body: formData
      });

      if (response.ok) {
        alert(`Orchid ${isUpdate ? 'updated' : 'added'} successfully!`);
        setShowAddModal(false);
        setEditingItem(null);
        setOrchidForm({
          orchidName: '',
          orchidDescription: '',
          price: '',
          categoryId: '',
          isNatural: false,
          orchidImage: null
        });
        fetchOrchids();
      } else {
        alert(`Failed to ${isUpdate ? 'update' : 'add'} orchid`);
      }
    } catch (error) {
      console.error('Error with orchid:', error);
      alert(`Error ${editingItem ? 'updating' : 'adding'} orchid`);
    }
  };

  // Add/Update category
  const handleCategorySubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      const isUpdate = editingItem !== null;
      const endpoint = isUpdate ? API_CONFIG.ENDPOINTS.UPDATE_CATEGORY : API_CONFIG.ENDPOINTS.INSERT_CATEGORY;

      const payload = {
        ...(isUpdate && { categoryId: editingItem.categoryId }),
        categoryName: categoryForm.categoryName,
        ...(categoryForm.parentCategoryId && { parentCategoryId: parseInt(categoryForm.parentCategoryId) })
      };

      const response = await fetch(buildApiUrl(endpoint), {
        method: isUpdate ? 'PUT' : 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`
        },
        body: JSON.stringify(payload)
      });

      if (response.ok) {
        alert(`Category ${isUpdate ? 'updated' : 'added'} successfully!`);
        setShowAddModal(false);
        setEditingItem(null);
        setCategoryForm({
          categoryName: '',
          parentCategoryId: ''
        });
        fetchCategories();
      } else {
        alert(`Failed to ${isUpdate ? 'update' : 'add'} category`);
      }
    } catch (error) {
      console.error('Error with category:', error);
      alert(`Error ${editingItem ? 'updating' : 'adding'} category`);
    }
  };

  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      await Promise.all([
        fetchStats(),
        fetchBestSelling(),
        fetchOrders(),
        fetchOrchids(),
        fetchCategories()
      ]);
      setLoading(false);
    };

    if (accessToken) {
      loadData();
    }
  }, [accessToken]);

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-green-600 mx-auto"></div>
          <p className="mt-4 text-lg text-gray-600">Đang tải bảng điều khiển quản trị...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-6">
            <h1 className="text-3xl font-bold text-gray-900">Bảng Điều Khiển Quản Trị</h1>
          </div>
        </div>
      </div>

      {/* Navigation Tabs */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 mt-6">
        <div className="border-b border-gray-200">
          <nav className="-mb-px flex space-x-8">
            {[
              { id: 'overview', name: 'Tổng Quan' },
              { id: 'orchids', name: 'Quản Lý Hoa Lan' },
              { id: 'categories', name: 'Quản Lý Danh Mục' },
              { id: 'orders', name: 'Quản Lý Đơn Hàng' }
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
                {tab.name}
              </button>
            ))}
          </nav>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        {activeTab === 'overview' && (
          <div className="space-y-6">
            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
              <div className="bg-white overflow-hidden shadow rounded-lg">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="flex-shrink-0">
                      <div className="w-8 h-8 bg-green-500 rounded-md flex items-center justify-center">
                        <span className="text-white font-bold">Đ</span>
                      </div>
                    </div>
                    <div className="ml-5 w-0 flex-1">
                      <dl>
                        <dt className="text-sm font-medium text-gray-500 truncate">Tổng Đơn Hàng</dt>
                        <dd className="text-lg font-medium text-gray-900">{stats?.totalOrders || 0}</dd>
                      </dl>
                    </div>
                  </div>
                </div>
              </div>

              <div className="bg-white overflow-hidden shadow rounded-lg">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="flex-shrink-0">
                      <div className="w-8 h-8 bg-blue-500 rounded-md flex items-center justify-center">
                        <span className="text-white font-bold">₫</span>
                      </div>
                    </div>
                    <div className="ml-5 w-0 flex-1">
                      <dl>
                        <dt className="text-sm font-medium text-gray-500 truncate">Tổng Doanh Thu</dt>
                        <dd className="text-lg font-medium text-gray-900">{stats?.totalRevenue || 0} VNĐ</dd>
                      </dl>
                    </div>
                  </div>
                </div>
              </div>

              <div className="bg-white overflow-hidden shadow rounded-lg">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="flex-shrink-0">
                      <div className="w-8 h-8 bg-purple-500 rounded-md flex items-center justify-center">
                        <span className="text-white font-bold">H</span>
                      </div>
                    </div>
                    <div className="ml-5 w-0 flex-1">
                      <dl>
                        <dt className="text-sm font-medium text-gray-500 truncate">Tổng Sản Phẩm</dt>
                        <dd className="text-lg font-medium text-gray-900">{orchids.length}</dd>
                      </dl>
                    </div>
                  </div>
                </div>
              </div>

              <div className="bg-white overflow-hidden shadow rounded-lg">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="flex-shrink-0">
                      <div className="w-8 h-8 bg-yellow-500 rounded-md flex items-center justify-center">
                        <span className="text-white font-bold">D</span>
                      </div>
                    </div>
                    <div className="ml-5 w-0 flex-1">
                      <dl>
                        <dt className="text-sm font-medium text-gray-500 truncate">Danh Mục</dt>
                        <dd className="text-lg font-medium text-gray-900">{categories.length}</dd>
                      </dl>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Best Selling Orchids */}
            <div className="bg-white shadow rounded-lg">
              <div className="px-6 py-4 border-b border-gray-200">
                <h3 className="text-lg font-medium text-gray-900">Hoa Lan Bán Chạy Nhất</h3>
              </div>
              <div className="p-6">
                {Array.isArray(bestSelling) && bestSelling.length > 0 ? (
                  <div className="space-y-4">
                    {bestSelling.map((item) => (
                      <div key={item.orchidId} className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                        <div>
                          <h4 className="font-medium text-gray-900">{item.orchidName}</h4>
                          <p className="text-sm text-gray-500">Đã bán: {item.totalSold}</p>
                        </div>
                        <div className="text-right">
                          <p className="font-medium text-gray-900">{item.totalRevenue} VNĐ</p>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-gray-500">Không có dữ liệu bán chạy</p>
                )}
              </div>
            </div>
          </div>
        )}

        {activeTab === 'orchids' && (
          <div className="space-y-6">
            <div className="flex justify-between items-center">
              <h2 className="text-2xl font-bold text-gray-900">Quản Lý Hoa Lan</h2>
              <button
                onClick={() => {
                  setActiveTab('orchids');
                  setEditingItem(null);
                  setShowAddModal(true);
                }}
                className="bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700"
              >
                + Thêm Hoa Lan Mới
              </button>
            </div>

            <div className="bg-white shadow rounded-lg overflow-hidden">
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Tên
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Mô Tả
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Giá
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Danh Mục
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Tự Nhiên
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Hành Động
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {Array.isArray(orchids) && orchids.length > 0 ? (
                      orchids.map((orchid) => (
                        <tr key={orchid.orchidId}>
                          <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                            {orchid.orchidName}
                          </td>
                          <td className="px-6 py-4 text-sm text-gray-500">
                            {orchid.orchidDescription}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                            ${orchid.price}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {getCategoryName(orchid.categoryId)}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {orchid.isNatural ? 'Có' : 'Không'}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm font-medium space-x-2">
                            <button
                              onClick={() => {
                                setEditingItem(orchid);
                                setOrchidForm({
                                  orchidName: orchid.orchidName,
                                  orchidDescription: orchid.orchidDescription,
                                  price: orchid.price.toString(),
                                  categoryId: orchid.categoryId.toString(),
                                  isNatural: orchid.isNatural,
                                  orchidImage: null
                                });
                                setShowAddModal(true);
                              }}
                              className="text-indigo-600 hover:text-indigo-900"
                            >
                              Chỉnh Sửa
                            </button>
                            <button
                              onClick={() => deleteOrchid(orchid.orchidId)}
                              className="text-red-600 hover:text-red-900"
                            >
                              Xóa
                            </button>
                          </td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan={6} className="px-6 py-4 text-center text-gray-500">
                          Không tìm thấy hoa lan
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>

              {/* Orchid Pagination */}
              <div className="px-4 py-3 flex items-center justify-between sm:px-6">
                <div className="flex-1 flex justify-between sm:hidden">
                  <button
                    onClick={() => handleOrchidPageChange(orchidPagination.currentPage - 1)}
                    disabled={orchidPagination.currentPage === 1}
                    className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                  >
                    Previous
                  </button>
                  <button
                    onClick={() => handleOrchidPageChange(orchidPagination.currentPage + 1)}
                    disabled={orchidPagination.currentPage === orchidPagination.totalPages}
                    className="relative ml-3 inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                  >
                    Next
                  </button>
                </div>
                <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
                  <div>
                    <p className="text-sm text-gray-700">
                      Showing page <span className="font-medium">{orchidPagination.currentPage}</span> of{' '}
                      <span className="font-medium">{orchidPagination.totalPages}</span>
                    </p>
                  </div>
                  <div>
                    <PaginationComponent
                      currentPage={orchidPagination.currentPage}
                      totalPages={orchidPagination.totalPages}
                      onPageChange={handleOrchidPageChange}
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'categories' && (
          <div className="space-y-6">
            <div className="flex justify-between items-center">
              <h2 className="text-2xl font-bold text-gray-900">Quản Lý Danh Mục</h2>
              <button
                onClick={() => {
                  setActiveTab('categories');
                  setEditingItem(null);
                  setShowAddModal(true);
                }}
                className="bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700"
              >
                + Thêm Danh Mục Mới
              </button>
            </div>

            <div className="bg-white shadow rounded-lg overflow-hidden">
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        ID
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Tên
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Danh Mục Cha
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Hành Động
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {Array.isArray(categories) && categories.length > 0 ? (
                      categories.map((category) => (
                        <tr key={category.categoryId}>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                            {category.categoryId}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                            {category.categoryName}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {getParentCategoryName(category.parentCategoryId)}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm font-medium space-x-2">
                            <button
                              onClick={() => {
                                setEditingItem(category);
                                setCategoryForm({
                                  categoryName: category.categoryName,
                                  parentCategoryId: category.parentCategoryId?.toString() || ''
                                });
                                setShowAddModal(true);
                              }}
                              className="text-indigo-600 hover:text-indigo-900"
                            >
                              Chỉnh Sửa
                            </button>
                            <button
                              onClick={() => deleteCategory(category.categoryId)}
                              className="text-red-600 hover:text-red-900"
                            >
                              Xóa
                            </button>
                          </td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan={4} className="px-6 py-4 text-center text-gray-500">
                          Không tìm thấy danh mục
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>

              {/* Category Pagination */}
              <div className="px-4 py-3 flex items-center justify-between sm:px-6">
                <div className="flex-1 flex justify-between sm:hidden">
                  <button
                    onClick={() => handleCategoryPageChange(categoryPagination.currentPage - 1)}
                    disabled={categoryPagination.currentPage === 1}
                    className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                  >
                    Previous
                  </button>
                  <button
                    onClick={() => handleCategoryPageChange(categoryPagination.currentPage + 1)}
                    disabled={categoryPagination.currentPage === categoryPagination.totalPages}
                    className="relative ml-3 inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                  >
                    Next
                  </button>
                </div>
                <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
                  <div>
                    <p className="text-sm text-gray-700">
                      Showing page <span className="font-medium">{categoryPagination.currentPage}</span> of{' '}
                      <span className="font-medium">{categoryPagination.totalPages}</span>
                    </p>
                  </div>
                  <div>
                    <PaginationComponent
                      currentPage={categoryPagination.currentPage}
                      totalPages={categoryPagination.totalPages}
                      onPageChange={handleCategoryPageChange}
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'orders' && (
          <div className="space-y-6">
            <h2 className="text-2xl font-bold text-gray-900">Quản Lý Đơn Hàng</h2>

            <div className="bg-white shadow rounded-lg overflow-hidden">
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Mã Đơn Hàng
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Khách Hàng
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Ngày Đặt
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Trạng Thái
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Tổng Tiền
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {Array.isArray(orders) && orders.length > 0 ? (
                      orders.map((order) => (
                        <tr key={order.id}>
                          <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                            #{order.id}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {order.customerEmail}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {new Date(order.orderDate).toLocaleDateString('vi-VN')}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                              order.orderStatus === 'Completed' ? 'bg-green-100 text-green-800' :
                              order.orderStatus === 'Pending' ? 'bg-yellow-100 text-yellow-800' :
                              'bg-red-100 text-red-800'
                            }`}>
                              {order.orderStatus === 'Completed' ? 'Hoàn Thành' :
                               order.orderStatus === 'Pending' ? 'Đang Xử Lý' :
                               order.orderStatus === 'Processing' ? 'Đang Xử Lý' :
                               order.orderStatus === 'Cancelled' ? 'Đã Hủy' : order.orderStatus}
                            </span>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                            {order.totalAmount} VNĐ
                          </td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan={5} className="px-6 py-4 text-center text-gray-500">
                          Không tìm thấy đơn hàng
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Add/Edit Modal */}
      {showAddModal && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="mt-3">
              <h3 className="text-lg font-medium text-gray-900 mb-4">
                {editingItem ? 'Chỉnh Sửa' : 'Thêm'} {activeTab === 'orchids' ? 'Hoa Lan' : 'Danh Mục'}
              </h3>

              {activeTab === 'orchids' ? (
                <form onSubmit={handleOrchidSubmit} className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Tên Hoa Lan</label>
                    <input
                      type="text"
                      value={orchidForm.orchidName}
                      onChange={(e) => setOrchidForm({...orchidForm, orchidName: e.target.value})}
                      className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Mô Tả</label>
                    <textarea
                      value={orchidForm.orchidDescription}
                      onChange={(e) => setOrchidForm({...orchidForm, orchidDescription: e.target.value})}
                      className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                      rows={3}
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Giá (VNĐ)</label>
                    <input
                      type="number"
                      step="0.01"
                      value={orchidForm.price}
                      onChange={(e) => setOrchidForm({...orchidForm, price: e.target.value})}
                      className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Danh Mục</label>
                    <select
                      value={orchidForm.categoryId}
                      onChange={(e) => setOrchidForm({...orchidForm, categoryId: e.target.value})}
                      className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                      required
                    >
                      <option value="">Chọn danh mục</option>
                      {categories.map((category) => (
                        <option key={category.categoryId} value={category.categoryId.toString()}>
                          {category.categoryName}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={orchidForm.isNatural}
                        onChange={(e) => setOrchidForm({...orchidForm, isNatural: e.target.checked})}
                        className="mr-2"
                      />
                      <span className="text-sm font-medium text-gray-700">Hoa Lan Tự Nhiên</span>
                    </label>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Hình Ảnh</label>
                    <input
                      type="file"
                      accept="image/*"
                      onChange={(e) => setOrchidForm({...orchidForm, orchidImage: e.target.files?.[0] || null})}
                      className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                      {...(!editingItem && { required: true })}
                    />
                  </div>
                  <div className="flex space-x-2">
                    <button
                      type="submit"
                      className="flex-1 bg-green-600 text-white py-2 px-4 rounded-md hover:bg-green-700"
                    >
                      {editingItem ? 'Cập Nhật' : 'Thêm'} Hoa Lan
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        setShowAddModal(false);
                        setEditingItem(null);
                      }}
                      className="flex-1 bg-gray-300 text-gray-700 py-2 px-4 rounded-md hover:bg-gray-400"
                    >
                      Hủy
                    </button>
                  </div>
                </form>
              ) : (
                <form onSubmit={handleCategorySubmit} className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Tên Danh Mục</label>
                    <input
                      type="text"
                      value={categoryForm.categoryName}
                      onChange={(e) => setCategoryForm({...categoryForm, categoryName: e.target.value})}
                      className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Danh Mục Cha (Tùy chọn)</label>
                    <select
                      value={categoryForm.parentCategoryId}
                      onChange={(e) => setCategoryForm({...categoryForm, parentCategoryId: e.target.value})}
                      className="mt-1 block w-full border border-gray-300 rounded-md px-3 py-2"
                    >
                      <option value="">-- Không có danh mục cha --</option>
                      {categories.map((category) => (
                        <option key={category.categoryId} value={category.categoryId.toString()}>
                          {category.categoryName}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="flex space-x-2">
                    <button
                      type="submit"
                      className="flex-1 bg-green-600 text-white py-2 px-4 rounded-md hover:bg-green-700"
                    >
                      {editingItem ? 'Cập Nhật' : 'Thêm'} Danh Mục
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        setShowAddModal(false);
                        setEditingItem(null);
                      }}
                      className="flex-1 bg-gray-300 text-gray-700 py-2 px-4 rounded-md hover:bg-gray-400"
                    >
                      Hủy
                    </button>
                  </div>
                </form>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
