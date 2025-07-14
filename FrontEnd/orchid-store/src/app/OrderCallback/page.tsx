'use client';

import React, { useEffect, useState } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import { API_CONFIG, buildApiUrl } from '@/config/api';
import { useAuth } from '@/contexts/AuthContext';

interface PaymentCallbackResponse {
    success: boolean;
    messageId: string;
    message: string;
    response?: {
        paymentUrl: string;
        qrCodeUrl: string;
        deeplink: string;
        deeplinkWebInApp: string;
    };
    detailErrorList?: any;
}

export default function OrderCallbackPage() {
    const searchParams = useSearchParams();
    const router = useRouter();
    const { accessToken, isAuthenticated } = useAuth();
    const [loading, setLoading] = useState(true);
    const [status, setStatus] = useState<'processing' | 'success' | 'failed' | 'redirect'>('processing');
    const [message, setMessage] = useState('Đang xử lý thanh toán...');
    const [countdown, setCountdown] = useState(3);

    useEffect(() => {
        const processPaymentCallback = async () => {
            try {
                // Lấy thông tin từ URL parameters
                const orderInfo = searchParams.get('orderInfo');
                const errorCode = searchParams.get('errorCode');
                const partnerCode = searchParams.get('partnerCode');
                const requestId = searchParams.get('requestId');
                const amount = searchParams.get('amount');
                const orderId = searchParams.get('orderId');
                const transId = searchParams.get('transId');
                const messageParam = searchParams.get('message');
                const localMessage = searchParams.get('localMessage');

                console.log('Payment callback parameters:', {
                    orderInfo,
                    errorCode,
                    partnerCode,
                    requestId,
                    amount,
                    orderId,
                    transId,
                    message: messageParam,
                    localMessage
                });

                // Gọi API PaymentOrderCallback
                if (!accessToken) {
                    setStatus('failed');
                    setMessage('Không tìm thấy token xác thực. Vui lòng đăng nhập lại.');
                    return;
                }

                const response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.PAYMENT_CALLBACK), {
                    method: 'POST',
                    headers: {
                        'accept': '*/*',
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${accessToken}`
                    },
                    body: JSON.stringify({
                        orderInfo: orderInfo || '',
                        errorCode: errorCode || ''
                    })
                });

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                const data: PaymentCallbackResponse = await response.json();
                console.log('Payment callback response:', data);

                if (data.success) {
                    // Thanh toán thành công
                    setStatus('success');
                    setMessage('Thanh toán thành công! Đang chuyển về trang chủ...');

                    // Đếm ngược 3 giây rồi chuyển về trang chủ
                    let timeLeft = 3;
                    const timer = setInterval(() => {
                        timeLeft--;
                        setCountdown(timeLeft);
                        if (timeLeft <= 0) {
                            clearInterval(timer);
                            router.push('/home');
                        }
                    }, 1000);
                } else {
                    // Thanh toán chưa hoàn tất, cần redirect đến paymentUrl
                    if (data.response?.paymentUrl) {
                        setStatus('redirect');
                        setMessage('Đang chuyển đến trang thanh toán...');

                        // Chờ 2 giây rồi redirect
                        setTimeout(() => {
                            window.location.href = data.response!.paymentUrl;
                        }, 2000);
                    } else {
                        setStatus('failed');
                        setMessage(data.message || 'Có lỗi xảy ra trong quá trình thanh toán');
                    }
                }
            } catch (error) {
                console.error('Error processing payment callback:', error);
                setStatus('failed');
                setMessage('Có lỗi xảy ra khi xử lý thanh toán. Vui lòng thử lại.');
            } finally {
                setLoading(false);
            }
        };

        processPaymentCallback();
    }, [searchParams, router, accessToken]);

    const getStatusIcon = () => {
        switch (status) {
            case 'processing':
                return (
                    <div className="animate-spin rounded-full h-16 w-16 border-4 border-purple-200 border-t-purple-600 mx-auto"></div>
                );
            case 'success':
                return (
                    <div className="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-green-100">
                        <svg className="h-8 w-8 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                        </svg>
                    </div>
                );
            case 'failed':
                return (
                    <div className="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-red-100">
                        <svg className="h-8 w-8 text-red-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </div>
                );
            case 'redirect':
                return (
                    <div className="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-blue-100">
                        <svg className="h-8 w-8 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
                        </svg>
                    </div>
                );
            default:
                return null;
        }
    };

    const getStatusColor = () => {
        switch (status) {
            case 'processing':
                return 'text-purple-600';
            case 'success':
                return 'text-green-600';
            case 'failed':
                return 'text-red-600';
            case 'redirect':
                return 'text-blue-600';
            default:
                return 'text-gray-600';
        }
    };

    const getBackgroundGradient = () => {
        switch (status) {
            case 'success':
                return 'from-green-50 via-emerald-50 to-white';
            case 'failed':
                return 'from-red-50 via-pink-50 to-white';
            case 'redirect':
                return 'from-blue-50 via-indigo-50 to-white';
            default:
                return 'from-purple-50 via-pink-50 to-white';
        }
    };

    return (
        <div className={`min-h-screen bg-gradient-to-br ${getBackgroundGradient()} flex items-center justify-center`}>
            <div className="max-w-md w-full mx-4">
                <div className="bg-white rounded-2xl shadow-xl p-8 text-center">
                    {/* Status Icon */}
                    <div className="mb-6">
                        {getStatusIcon()}
                    </div>

                    {/* Title */}
                    <h1 className={`text-2xl font-bold mb-4 ${getStatusColor()}`}>
                        {status === 'processing' && 'Đang xử lý thanh toán'}
                        {status === 'success' && 'Thanh toán thành công!'}
                        {status === 'failed' && 'Thanh toán thất bại'}
                        {status === 'redirect' && 'Chuyển hướng thanh toán'}
                    </h1>

                    {/* Message */}
                    <p className="text-gray-600 mb-6 leading-relaxed">
                        {message}
                    </p>

                    {/* Countdown for success status */}
                    {status === 'success' && (
                        <div className="mb-6">
                            <div className="inline-flex items-center justify-center w-12 h-12 bg-green-100 rounded-full">
                                <span className="text-xl font-bold text-green-600">{countdown}</span>
                            </div>
                        </div>
                    )}

                    {/* Loading indicator for processing/redirect */}
                    {(status === 'processing' || status === 'redirect') && (
                        <div className="mb-6">
                            <div className="flex justify-center">
                                <div className="flex space-x-1">
                                    <div className="w-2 h-2 bg-purple-600 rounded-full animate-bounce"></div>
                                    <div className="w-2 h-2 bg-purple-600 rounded-full animate-bounce" style={{ animationDelay: '0.1s' }}></div>
                                    <div className="w-2 h-2 bg-purple-600 rounded-full animate-bounce" style={{ animationDelay: '0.2s' }}></div>
                                </div>
                            </div>
                        </div>
                    )}

                    {/* Action buttons */}
                    <div className="space-y-3">
                        {status === 'failed' && (
                            <>
                                <button
                                    onClick={() => router.push('/home')}
                                    className="w-full bg-gradient-to-r from-purple-500 to-pink-500 text-white px-6 py-3 rounded-lg font-medium hover:from-purple-600 hover:to-pink-600 transition-all duration-300"
                                >
                                    Quay về trang chủ
                                </button>
                                <button
                                    onClick={() => window.location.reload()}
                                    className="w-full bg-white border border-gray-300 text-gray-700 px-6 py-3 rounded-lg font-medium hover:bg-gray-50 transition-all duration-300"
                                >
                                    Thử lại
                                </button>
                            </>
                        )}

                        {status === 'success' && (
                            <button
                                onClick={() => router.push('/home')}
                                className="w-full bg-gradient-to-r from-green-500 to-emerald-500 text-white px-6 py-3 rounded-lg font-medium hover:from-green-600 hover:to-emerald-600 transition-all duration-300"
                            >
                                Về trang chủ ngay
                            </button>
                        )}

                        {status === 'redirect' && (
                            <button
                                onClick={() => router.push('/home')}
                                className="w-full bg-white border border-gray-300 text-gray-700 px-6 py-3 rounded-lg font-medium hover:bg-gray-50 transition-all duration-300"
                            >
                                Hủy và quay về trang chủ
                            </button>
                        )}
                    </div>

                    {/* Additional info */}
                    <div className="mt-6 pt-6 border-t border-gray-200">
                        <p className="text-sm text-gray-500">
                            Nếu bạn gặp bất kỳ vấn đề nào, vui lòng liên hệ với chúng tôi để được hỗ trợ.
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
}
