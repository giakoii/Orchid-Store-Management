'use client';

import React, { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { API_CONFIG, buildApiUrl } from '@/config/api';
import { Orchid } from '@/types/orchid';
import { useCart } from '@/contexts/CartContext';
import Header from '@/components/Header';
import { formatCurrency } from '@/utils/format';

interface OrchidDetailResponse {
    success: boolean;
    response: Orchid;
}

export default function OrchidDetailPage() {
    const params = useParams();
    const router = useRouter();
    const { addToCart } = useCart();

    const [orchid, setOrchid] = useState<Orchid | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [quantity, setQuantity] = useState(1);
    const [addingToCart, setAddingToCart] = useState(false);
    const [addToCartMessage, setAddToCartMessage] = useState<string | null>(null);

    const orchidId = Array.isArray(params.id) ? params.id[0] : params.id;

    useEffect(() => {
        if (orchidId) {
            fetchOrchidDetail(parseInt(orchidId));
        }
    }, [orchidId]);

    const fetchOrchidDetail = async (id: number) => {
        try {
            setLoading(true);
            const response = await fetch(buildApiUrl(`${API_CONFIG.ENDPOINTS.SELECT_ORCHIDS}/${id}`));
            const data: OrchidDetailResponse = await response.json();

            if (data.success) {
                setOrchid(data.response);
            } else {
                setError('Không thể tải thông tin hoa lan');
            }
        } catch (err) {
            setError('Có lỗi xảy ra khi tải thông tin hoa lan');
            console.error('Error fetching orchid detail:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleAddToCart = async () => {
        if (!orchid) return;

        setAddingToCart(true);
        try {
            addToCart(orchid, quantity);
            setAddToCartMessage('Đã thêm vào giỏ hàng!');
            setTimeout(() => setAddToCartMessage(null), 3000);
        } finally {
            setAddingToCart(false);
        }
    };

    const handleQuantityChange = (newQuantity: number) => {
        if (newQuantity >= 1) {
            setQuantity(newQuantity);
        }
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-purple-50 via-pink-50 to-white">
                <Header />
                <div className="flex justify-center items-center py-32">
                    <div className="animate-spin rounded-full h-16 w-16 border-4 border-purple-200 border-t-purple-600"></div>
                </div>
            </div>
        );
    }

    if (error || !orchid) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-purple-50 via-pink-50 to-white">
                <Header />
                <div className="max-w-7xl mx-auto px-4 py-16">
                    <div className="text-center">
                        <h1 className="text-2xl font-bold text-gray-900 mb-4">Không tìm thấy hoa lan</h1>
                        <p className="text-gray-600 mb-8">{error}</p>
                        <button
                            onClick={() => router.push('/home')}
                            className="bg-gradient-to-r from-purple-500 to-pink-500 text-white px-6 py-3 rounded-lg hover:from-purple-600 hover:to-pink-600 transition-all duration-300"
                        >
                            Quay lại trang chủ
                        </button>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gradient-to-br from-purple-50 via-pink-50 to-white">
            <Header />

            <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {/* Breadcrumb */}
                <nav className="flex mb-8" aria-label="Breadcrumb">
                    <ol className="flex items-center space-x-2">
                        <li>
                            <button
                                onClick={() => router.push('/home')}
                                className="text-purple-600 hover:text-purple-800 transition-colors"
                            >
                                Trang chủ
                            </button>
                        </li>
                        <li>
                            <span className="text-gray-400 mx-2">/</span>
                            <span className="text-gray-600">{orchid.orchidName}</span>
                        </li>
                    </ol>
                </nav>

                {/* Success Message */}
                {addToCartMessage && (
                    <div className="fixed top-24 right-4 z-50 bg-green-500 text-white px-6 py-3 rounded-lg shadow-lg animate-bounce">
                        {addToCartMessage}
                    </div>
                )}

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-12">
                    {/* Image Section */}
                    <div className="space-y-4">
                        <div className="aspect-square rounded-2xl overflow-hidden shadow-xl">
                            <img
                                src={orchid.orchidUrl}
                                alt={orchid.orchidName}
                                className="w-full h-full object-cover hover:scale-105 transition-transform duration-500"
                                onError={(e) => {
                                    const target = e.target as HTMLImageElement;
                                    target.src = '/placeholder-orchid.jpg';
                                }}
                            />
                        </div>
                    </div>

                    {/* Details Section */}
                    <div className="space-y-6">
                        {/* Basic Info */}
                        <div>
                            <h1 className="text-4xl font-bold text-gray-900 mb-4">{orchid.orchidName}</h1>
                            <div className="flex items-center space-x-4 mb-6">
                                <span className={`px-4 py-2 rounded-full text-sm font-medium ${
                                    orchid.isNatural 
                                        ? 'bg-green-100 text-green-800' 
                                        : 'bg-blue-100 text-blue-800'
                                }`}>
                                    {orchid.isNatural ? 'Hoa lan tự nhiên' : 'Hoa lan lai tạo'}
                                </span>
                                <span className="px-4 py-2 rounded-full text-sm font-medium bg-purple-100 text-purple-800">
                                    {orchid.categoryName}
                                </span>
                            </div>
                        </div>

                        {/* Price */}
                        <div className="border-t border-b border-gray-200 py-6">
                            <div className="text-4xl font-bold bg-gradient-to-r from-purple-600 to-pink-600 bg-clip-text text-transparent">
                                {formatCurrency(orchid.price)}
                            </div>
                            <p className="text-gray-600 mt-2">Giá đã bao gồm VAT</p>
                        </div>

                        {/* Description */}
                        <div>
                            <h3 className="text-xl font-semibold text-gray-900 mb-3">Mô tả</h3>
                            <p className="text-gray-700 leading-relaxed">{orchid.orchidDescription}</p>
                        </div>

                        {/* Specifications */}
                        <div className="bg-gray-50 rounded-lg p-6">
                            <h3 className="text-xl font-semibold text-gray-900 mb-4">Thông số kỹ thuật</h3>
                            <div className="grid grid-cols-1 gap-3">
                                <div className="flex justify-between">
                                    <span className="text-gray-600">Mã sản phẩm:</span>
                                    <span className="font-medium">#{orchid.orchidId}</span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-gray-600">Loại:</span>
                                    <span className="font-medium">{orchid.isNatural ? 'Tự nhiên' : 'Lai tạo'}</span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-gray-600">Danh mục:</span>
                                    <span className="font-medium">{orchid.categoryName}</span>
                                </div>
                            </div>
                        </div>

                        {/* Add to Cart Section */}
                        <div className="bg-white rounded-lg border-2 border-purple-100 p-6 shadow-lg">
                            <div className="flex items-center space-x-6 mb-6">
                                <div className="flex items-center space-x-3">
                                    <label htmlFor="quantity" className="text-gray-700 font-medium">
                                        Số lượng:
                                    </label>
                                    <div className="flex items-center border border-gray-300 rounded-lg">
                                        <button
                                            onClick={() => handleQuantityChange(quantity - 1)}
                                            className="px-3 py-2 text-gray-600 hover:bg-gray-100 transition-colors rounded-l-lg"
                                            disabled={quantity <= 1}
                                        >
                                            -
                                        </button>
                                        <input
                                            id="quantity"
                                            type="number"
                                            min="1"
                                            value={quantity}
                                            onChange={(e) => handleQuantityChange(parseInt(e.target.value) || 1)}
                                            className="w-16 px-3 py-2 text-center border-0 focus:ring-0"
                                        />
                                        <button
                                            onClick={() => handleQuantityChange(quantity + 1)}
                                            className="px-3 py-2 text-gray-600 hover:bg-gray-100 transition-colors rounded-r-lg"
                                        >
                                            +
                                        </button>
                                    </div>
                                </div>
                                <div className="text-lg font-semibold text-gray-900">
                                    Tổng: {formatCurrency(orchid.price * quantity)}
                                </div>
                            </div>

                            <button
                                onClick={handleAddToCart}
                                disabled={addingToCart}
                                className="w-full bg-gradient-to-r from-purple-500 to-pink-500 text-white px-8 py-4 rounded-lg font-semibold text-lg hover:from-purple-600 hover:to-pink-600 focus:outline-none focus:ring-4 focus:ring-purple-300 disabled:opacity-50 disabled:cursor-not-allowed transform transition-all duration-300 hover:scale-105 active:scale-95"
                            >
                                {addingToCart ? (
                                    <div className="flex items-center justify-center">
                                        <div className="animate-spin rounded-full h-5 w-5 border-2 border-white border-t-transparent mr-2"></div>
                                        Đang thêm...
                                    </div>
                                ) : (
                                    'Thêm vào giỏ hàng'
                                )}
                            </button>
                        </div>

                        {/* Additional Actions */}
                        <div className="flex space-x-4">
                            <button
                                onClick={() => router.push('/home')}
                                className="flex-1 bg-white border border-gray-300 text-gray-700 px-6 py-3 rounded-lg font-medium hover:bg-gray-50 transition-colors"
                            >
                                Tiếp tục mua sắm
                            </button>
                        </div>
                    </div>
                </div>

                {/* Related Products Section - Placeholder */}
                <div className="mt-16">
                    <h2 className="text-2xl font-bold text-gray-900 mb-8">Sản phẩm liên quan</h2>
                    <div className="text-center py-8 text-gray-500">
                        Tính năng sản phẩm liên quan sẽ được phát triển trong tương lai
                    </div>
                </div>
            </main>
        </div>
    );
}
