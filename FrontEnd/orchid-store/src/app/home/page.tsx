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

    // Fetch orchids
    const fetchOrchids = async (categoryId?: number, page: number = 1) => {
        try {
            setLoading(true);
            const params: Record<string, string | number> = {
                pageNumber: page,
                pageSize: pageSize
            };
            if (categoryId) {
                params.CategoryId = categoryId;
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
        fetchOrchids(categoryId || undefined, 1);
    };

    const handlePageChange = (page: number) => {
        if (page >= 1 && page <= totalPages) {
            setCurrentPage(page);
            fetchOrchids(selectedCategory || undefined, page);
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

    return (
        <div className="min-h-screen bg-gradient-to-br from-purple-50 via-pink-50 to-white">
            {/* Header with Auth */}
            <Header />

            <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {/* Category Filter */}
                <div className="mb-8">
                    <h2 className="text-2xl font-semibold text-gray-800 mb-4">Danh mục</h2>
                    <div className="flex flex-wrap gap-3">
                        <button
                            onClick={() => handleCategoryChange(null)}
                            className={`px-6 py-3 rounded-full transition-all duration-300 ${
                                selectedCategory === null
                                    ? 'bg-gradient-to-r from-purple-500 to-pink-500 text-white shadow-lg transform scale-105'
                                    : 'bg-white text-gray-700 border border-purple-200 hover:border-purple-400 hover:bg-purple-50'
                            }`}
                        >
                            Tất cả
                        </button>
                        {categories.map((category) => (
                            <button
                                key={category.categoryId}
                                onClick={() => handleCategoryChange(category.categoryId)}
                                className={`px-6 py-3 rounded-full transition-all duration-300 ${
                                    selectedCategory === category.categoryId
                                        ? 'bg-gradient-to-r from-purple-500 to-pink-500 text-white shadow-lg transform scale-105'
                                        : 'bg-white text-gray-700 border border-purple-200 hover:border-purple-400 hover:bg-purple-50'
                                }`}
                            >
                                {category.categoryName}
                            </button>
                        ))}
                    </div>
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
