'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { API_CONFIG, buildApiUrl } from '@/config/api';
import { Orchid, Category, OrchidResponse, CategoryResponse } from '@/types/orchid';
import { useCart } from '@/contexts/CartContext';
import Header from '@/components/Header';
import { formatCurrency } from '@/utils/format';

export default function HomePage() {
    const [orchids, setOrchids] = useState<Orchid[]>([]);
    const [categories, setCategories] = useState<Category[]>([]);
    const [selectedCategory, setSelectedCategory] = useState<number | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Pagination state
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalCount, setTotalCount] = useState(0);
    const [pageSize] = useState(12); // 12 orchids per page

    // Filter state
    const [filters, setFilters] = useState({
        categoryId: null as number | null,
        isNatural: null as boolean | null,
        minPrice: '' as string,
        maxPrice: '' as string,
        searchName: '' as string
    });
    const [showFilters, setShowFilters] = useState(false);

    // Cart functionality
    const { addToCart } = useCart();
    const [addingToCart, setAddingToCart] = useState<number | null>(null);

    // Router instance
    const router = useRouter();

    // Fetch categories
    const fetchCategories = async () => {
        try {
            const response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.SELECT_CATEGORIES));
            const data: CategoryResponse = await response.json();
            if (data.success) {
                setCategories(data.response.categories);
            }
        } catch (err) {
            console.error('Error fetching categories:', err);
        }
    };

    // Fetch orchids with filters
    const fetchOrchids = async (page: number = 1, applyFilters: boolean = false) => {
        try {
            setLoading(true);
            const params: Record<string, string | number> = {
                pageNumber: page,
                pageSize: pageSize
            };

            // Apply filters
            if (applyFilters) {
                if (filters.categoryId) params.CategoryId = filters.categoryId;
                if (filters.isNatural !== null) params.IsNatural = filters.isNatural.toString();
                if (filters.minPrice) params.MinPrice = parseFloat(filters.minPrice);
                if (filters.maxPrice) params.MaxPrice = parseFloat(filters.maxPrice);
                if (filters.searchName) params.SearchName = filters.searchName;
            } else {
                // Use legacy category filter for backward compatibility
                if (selectedCategory) params.CategoryId = selectedCategory;
            }

            const response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.SELECT_ORCHIDS, params));
            const data: OrchidResponse = await response.json();
            if (data.success) {
                setOrchids(data.response.orchids);
                setTotalCount(data.response.totalCount);
                setCurrentPage(data.response.pageNumber);
                setTotalPages(Math.ceil(data.response.totalCount / data.response.pageSize));
            }
        } catch (err) {
            setError('Không thể tải danh sách hoa lan');
            console.error('Error fetching orchids:', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchCategories();
        fetchOrchids();
    }, []);

    const handleCategoryChange = (categoryId: number | null) => {
        setSelectedCategory(categoryId);
        setCurrentPage(1); // Reset to first page when changing category
        fetchOrchids(1, false);
    };

    const handlePageChange = (page: number) => {
        if (page >= 1 && page <= totalPages) {
            setCurrentPage(page);
            fetchOrchids(page, false);
            // Scroll to top when page changes
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    };

    const handleAddToCart = async (orchid: Orchid) => {
        setAddingToCart(orchid.orchidId);
        try {
            addToCart(orchid, 1);
            // Add a small delay for better UX
            await new Promise(resolve => setTimeout(resolve, 500));
        } finally {
            setAddingToCart(null);
        }
    };

    // Handle filter changes
    const handleFilterChange = (filterType: string, value: any) => {
        setFilters(prev => ({
            ...prev,
            [filterType]: value
        }));
    };

    // Apply filters
    const applyFilters = () => {
        setCurrentPage(1);
        fetchOrchids(1, true);
    };

    // Clear filters
    const clearFilters = () => {
        setFilters({
            categoryId: null,
            isNatural: null,
            minPrice: '',
            maxPrice: '',
            searchName: ''
        });
        setSelectedCategory(null);
        setCurrentPage(1);
        fetchOrchids(1, false);
    };

    return (
        <div className="min-h-screen bg-gradient-to-br from-purple-50 via-pink-50 to-white">
            {/* Header with Auth */}
            <Header />

            <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {/* Advanced Filters */}
                <div className="mb-8 bg-white rounded-2xl shadow-md overflow-hidden">
                    <div className="px-6 py-4 border-b border-gray-200">
                        <div className="flex items-center justify-between">
                            <h3 className="text-lg font-semibold text-gray-800">Bộ lọc nâng cao</h3>
                            <button
                                onClick={() => setShowFilters(!showFilters)}
                                className="flex items-center space-x-2 text-purple-600 hover:text-purple-800 transition-colors"
                            >
                                <span>{showFilters ? 'Ẩn bộ lọc' : 'Hiển thị bộ lọc'}</span>
                                <svg
                                    className={`w-5 h-5 transform transition-transform ${showFilters ? 'rotate-180' : ''}`}
                                    fill="none"
                                    stroke="currentColor"
                                    viewBox="0 0 24 24"
                                >
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 9l-7 7-7-7" />
                                </svg>
                            </button>
                        </div>
                    </div>

                    {showFilters && (
                        <div className="p-6 space-y-6">
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                                {/* Search Name */}
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Tìm kiếm theo tên
                                    </label>
                                    <input
                                        type="text"
                                        value={filters.searchName}
                                        onChange={(e) => handleFilterChange('searchName', e.target.value)}
                                        placeholder="Nhập tên hoa lan..."
                                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-colors"
                                    />
                                </div>

                                {/* Category Filter */}
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Danh mục
                                    </label>
                                    <select
                                        value={filters.categoryId || ''}
                                        onChange={(e) => handleFilterChange('categoryId', e.target.value ? parseInt(e.target.value) : null)}
                                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-colors"
                                    >
                                        <option value="">Tất cả danh mục</option>
                                        {categories.map((category) => (
                                            <option key={category.categoryId} value={category.categoryId}>
                                                {category.categoryName}
                                            </option>
                                        ))}
                                    </select>
                                </div>

                                {/* Natural Type Filter */}
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Loại hoa lan
                                    </label>
                                    <select
                                        value={filters.isNatural === null ? '' : filters.isNatural.toString()}
                                        onChange={(e) => handleFilterChange('isNatural', e.target.value === '' ? null : e.target.value === 'true')}
                                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-colors"
                                    >
                                        <option value="">Tất cả loại</option>
                                        <option value="true">Tự nhiên</option>
                                        <option value="false">Lai tạo</option>
                                    </select>
                                </div>

                                {/* Min Price */}
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Giá tối thiểu (VNĐ)
                                    </label>
                                    <input
                                        type="number"
                                        value={filters.minPrice}
                                        onChange={(e) => handleFilterChange('minPrice', e.target.value)}
                                        placeholder="0"
                                        min="0"
                                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-colors"
                                    />
                                </div>

                                {/* Max Price */}
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Giá tối đa (VNĐ)
                                    </label>
                                    <input
                                        type="number"
                                        value={filters.maxPrice}
                                        onChange={(e) => handleFilterChange('maxPrice', e.target.value)}
                                        placeholder="999999999"
                                        min="0"
                                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-colors"
                                    />
                                </div>
                            </div>

                            {/* Filter Actions */}
                            <div className="flex flex-wrap gap-3 pt-4 border-t border-gray-200">
                                <button
                                    onClick={applyFilters}
                                    className="px-6 py-2 bg-gradient-to-r from-purple-500 to-pink-500 text-white rounded-lg hover:from-purple-600 hover:to-pink-600 transition-all duration-300 transform hover:scale-105 shadow-md"
                                >
                                    <div className="flex items-center space-x-2">
                                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z" />
                                        </svg>
                                        <span>Áp dụng bộ lọc</span>
                                    </div>
                                </button>
                                <button
                                    onClick={clearFilters}
                                    className="px-6 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition-all duration-300"
                                >
                                    <div className="flex items-center space-x-2">
                                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                        </svg>
                                        <span>Xóa bộ lọc</span>
                                    </div>
                                </button>
                            </div>

                            {/* Active Filters Display */}
                            {(filters.searchName || filters.categoryId || filters.isNatural !== null || filters.minPrice || filters.maxPrice) && (
                                <div className="pt-4 border-t border-gray-200">
                                    <h4 className="text-sm font-medium text-gray-700 mb-3">Bộ lọc đang áp dụng:</h4>
                                    <div className="flex flex-wrap gap-2">
                                        {filters.searchName && (
                                            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                                                Tên: "{filters.searchName}"
                                                <button
                                                    onClick={() => handleFilterChange('searchName', '')}
                                                    className="ml-2 text-purple-600 hover:text-purple-800"
                                                >
                                                    ×
                                                </button>
                                            </span>
                                        )}
                                        {filters.categoryId && (
                                            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                                                Danh mục: {categories.find(c => c.categoryId === filters.categoryId)?.categoryName}
                                                <button
                                                    onClick={() => handleFilterChange('categoryId', null)}
                                                    className="ml-2 text-blue-600 hover:text-blue-800"
                                                >
                                                    ×
                                                </button>
                                            </span>
                                        )}
                                        {filters.isNatural !== null && (
                                            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                                                Loại: {filters.isNatural ? 'Tự nhiên' : 'Lai tạo'}
                                                <button
                                                    onClick={() => handleFilterChange('isNatural', null)}
                                                    className="ml-2 text-green-600 hover:text-green-800"
                                                >
                                                    ×
                                                </button>
                                            </span>
                                        )}
                                        {filters.minPrice && (
                                            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                                                Giá tối thiểu: {formatCurrency(parseFloat(filters.minPrice))}
                                                <button
                                                    onClick={() => handleFilterChange('minPrice', '')}
                                                    className="ml-2 text-yellow-600 hover:text-yellow-800"
                                                >
                                                    ×
                                                </button>
                                            </span>
                                        )}
                                        {filters.maxPrice && (
                                            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800">
                                                Giá tối đa: {formatCurrency(parseFloat(filters.maxPrice))}
                                                <button
                                                    onClick={() => handleFilterChange('maxPrice', '')}
                                                    className="ml-2 text-red-600 hover:text-red-800"
                                                >
                                                    ×
                                                </button>
                                            </span>
                                        )}
                                    </div>
                                </div>
                            )}
                        </div>
                    )}
                </div>

                {/* Loading State */}
                {loading && (
                    <div className="flex justify-center items-center py-16">
                        <div className="animate-spin rounded-full h-16 w-16 border-4 border-purple-200 border-t-purple-600"></div>
                    </div>
                )}

                {/* Error State */}
                {error && (
                    <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
                        <p className="text-red-600">{error}</p>
                    </div>
                )}

                {/* Orchid Grid */}
                {!loading && !error && (
                    <div>
                        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                            {orchids.map((orchid) => (
                                <div
                                    key={orchid.orchidId}
                                    className="bg-white rounded-2xl shadow-md hover:shadow-xl transition-all duration-300 overflow-hidden group hover:transform hover:scale-105"
                                >
                                    <div className="relative overflow-hidden">
                                        <img
                                            src={orchid.orchidUrl}
                                            alt={orchid.orchidName}
                                            className="w-full h-64 object-cover group-hover:scale-110 transition-transform duration-500"
                                            onError={(e) => {
                                                const target = e.target as HTMLImageElement;
                                                target.src = '/placeholder-orchid.jpg';
                                            }}
                                        />
                                        <div className="absolute top-4 right-4">
                                            <span className={`px-3 py-1 rounded-full text-xs font-medium ${
                                                orchid.isNatural 
                                                    ? 'bg-green-100 text-green-800' 
                                                    : 'bg-blue-100 text-blue-800'
                                            }`}>
                                                {orchid.isNatural ? 'Tự nhiên' : 'Lai tạo'}
                                            </span>
                                        </div>
                                        <div className="absolute top-4 left-4">
                                            <span className="px-3 py-1 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                                                {orchid.categoryName}
                                            </span>
                                        </div>
                                    </div>

                                    <div className="p-6">
                                        <h3 className="text-xl font-semibold text-gray-800 mb-2 line-clamp-1">
                                            {orchid.orchidName}
                                        </h3>
                                        <p className="text-gray-600 mb-4 line-clamp-2">
                                            {orchid.orchidDescription}
                                        </p>
                                        <div className="flex items-center justify-between mb-4">
                                            <span className="text-2xl font-bold bg-gradient-to-r from-purple-600 to-pink-600 bg-clip-text text-transparent">
                                                {formatCurrency(orchid.price)}
                                            </span>
                                        </div>

                                        {/* Action buttons */}
                                        <div className="flex space-x-2">
                                            <button
                                                onClick={() => handleAddToCart(orchid)}
                                                disabled={addingToCart === orchid.orchidId}
                                                className="flex-1 bg-gradient-to-r from-purple-500 to-pink-500 text-white px-4 py-2 rounded-full hover:from-purple-600 hover:to-pink-600 transition-all duration-300 transform hover:scale-105 shadow-md disabled:opacity-70 disabled:cursor-not-allowed disabled:transform-none flex items-center justify-center"
                                            >
                                                {addingToCart === orchid.orchidId ? (
                                                    <div className="flex items-center">
                                                        <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                                                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                                                            <path className="opacity-75" fill="currentColor" d="m4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                                                        </svg>
                                                        Đang thêm...
                                                    </div>
                                                ) : (
                                                    <div className="flex items-center">
                                                        <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                                                        </svg>
                                                        Thêm vào giỏ
                                                    </div>
                                                )}
                                            </button>
                                            <button
                                                onClick={() => router.push(`/orchid/${orchid.orchidId}`)}
                                                className="px-4 py-2 border border-purple-300 text-purple-600 rounded-full hover:bg-purple-50 transition-all duration-300"
                                            >
                                                Chi tiết
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>

                        {/* Pagination */}
                        {totalPages > 1 && (
                            <div className="mt-12 flex flex-col items-center space-y-4">
                                {/* Results info */}
                                <div className="text-sm text-gray-600">
                                    Hiển thị {((currentPage - 1) * pageSize) + 1} - {Math.min(currentPage * pageSize, totalCount)} trong tổng số {totalCount} hoa lan
                                </div>

                                {/* Pagination controls */}
                                <div className="flex items-center space-x-1">
                                    {/* Previous button */}
                                    <button
                                        onClick={() => handlePageChange(currentPage - 1)}
                                        disabled={currentPage === 1}
                                        className="px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200"
                                    >
                                        <span className="sr-only">Trang trước</span>
                                        ←
                                    </button>

                                    {/* Page numbers */}
                                    {(() => {
                                        const pages = [];
                                        const maxVisiblePages = 5;
                                        let startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
                                        let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

                                        // Adjust start page if we're near the end
                                        if (endPage - startPage + 1 < maxVisiblePages) {
                                            startPage = Math.max(1, endPage - maxVisiblePages + 1);
                                        }

                                        // First page and ellipsis
                                        if (startPage > 1) {
                                            pages.push(
                                                <button
                                                    key={1}
                                                    onClick={() => handlePageChange(1)}
                                                    className="px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-700 hover:bg-purple-50 hover:border-purple-300 transition-all duration-200"
                                                >
                                                    1
                                                </button>
                                            );
                                            if (startPage > 2) {
                                                pages.push(
                                                    <span key="ellipsis1" className="px-2 py-2 text-gray-500">
                                                        ...
                                                    </span>
                                                );
                                            }
                                        }

                                        // Visible page numbers
                                        for (let i = startPage; i <= endPage; i++) {
                                            pages.push(
                                                <button
                                                    key={i}
                                                    onClick={() => handlePageChange(i)}
                                                    className={`px-3 py-2 rounded-lg border transition-all duration-200 ${
                                                        i === currentPage
                                                            ? 'bg-gradient-to-r from-purple-500 to-pink-500 text-white border-purple-500'
                                                            : 'border-gray-300 bg-white text-gray-700 hover:bg-purple-50 hover:border-purple-300'
                                                    }`}
                                                >
                                                    {i}
                                                </button>
                                            );
                                        }

                                        // Last page and ellipsis
                                        if (endPage < totalPages) {
                                            if (endPage < totalPages - 1) {
                                                pages.push(
                                                    <span key="ellipsis2" className="px-2 py-2 text-gray-500">
                                                        ...
                                                    </span>
                                                );
                                            }
                                            pages.push(
                                                <button
                                                    key={totalPages}
                                                    onClick={() => handlePageChange(totalPages)}
                                                    className="px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-700 hover:bg-purple-50 hover:border-purple-300 transition-all duration-200"
                                                >
                                                    {totalPages}
                                                </button>
                                            );
                                        }

                                        return pages;
                                    })()}

                                    {/* Next button */}
                                    <button
                                        onClick={() => handlePageChange(currentPage + 1)}
                                        disabled={currentPage === totalPages}
                                        className="px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200"
                                    >
                                        <span className="sr-only">Trang tiếp</span>
                                        →
                                    </button>
                                </div>

                                {/* Quick jump to page */}
                                {totalPages > 10 && (
                                    <div className="flex items-center space-x-2 text-sm">
                                        <span className="text-gray-600">Chuyển đến trang:</span>
                                        <input
                                            type="number"
                                            min={1}
                                            max={totalPages}
                                            className="w-16 px-2 py-1 border border-gray-300 rounded text-center focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500"
                                            onKeyDown={(e) => {
                                                if (e.key === 'Enter') {
                                                    const page = parseInt((e.target as HTMLInputElement).value);
                                                    if (page >= 1 && page <= totalPages) {
                                                        handlePageChange(page);
                                                        (e.target as HTMLInputElement).value = '';
                                                    }
                                                }
                                            }}
                                            placeholder={currentPage.toString()}
                                        />
                                        <span className="text-gray-600">/ {totalPages}</span>
                                    </div>
                                )}
                            </div>
                        )}

                    </div>
                )}

                {/* Empty State */}
                {!loading && !error && orchids.length === 0 && (
                    <div className="text-center py-16">
                        <div className="text-6xl mb-4">🌸</div>
                        <h3 className="text-2xl font-semibold text-gray-600 mb-2">
                            Không tìm thấy hoa lan nào
                        </h3>
                        <p className="text-gray-500">
                            Thử chọn danh mục khác hoặc liên hệ với chúng tôi để biết thêm thông tin.
                        </p>
                    </div>
                )}
            </main>

            {/* Footer */}
            <footer className="bg-gradient-to-r from-purple-800 to-pink-800 text-white py-8 mt-16">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <p className="text-lg">
                        © 2025 Orchid Garden - Nơi hội tụ những loài hoa lan đẹp nhất
                    </p>
                </div>
            </footer>
        </div>
    );
}
