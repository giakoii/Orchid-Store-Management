'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { useAuth } from '@/contexts/AuthContext';
import { useCart } from '@/contexts/CartContext';
import LoginModal from './LoginModal';
import RegisterModal from './RegisterModal';
import Cart from './Cart';
import OrderHistoryModal from './OrderHistory';

export default function Header() {
  const [showLoginModal, setShowLoginModal] = useState(false);
  const [showRegisterModal, setShowRegisterModal] = useState(false);
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [isCartOpen, setIsCartOpen] = useState(false);
  const [showOrderHistory, setShowOrderHistory] = useState(false);

  const { isAuthenticated, user, role, logout, authReady } = useAuth();
  const { cart } = useCart();

  if (!authReady) {
    return (
        <div className="text-center py-6">
          <span className="text-gray-500">Đang tải xác thực...</span>
        </div>
    );
  }

  const handleSwitchToRegister = () => {
    setShowLoginModal(false);
    setShowRegisterModal(true);
  };

  const handleSwitchToLogin = () => {
    setShowRegisterModal(false);
    setShowLoginModal(true);
  };

  const handleLogout = async () => {
    if (typeof logout !== 'function') {
      console.error('logout is not a function!', logout);
      return;
    }

    try {
      setShowUserMenu(false);
      await logout();
    } catch (error) {
      console.error('❌ Error in handleLogout:', error);
    }
  };

  return (
    <>
      <header className="bg-white/80 backdrop-blur-md shadow-lg border-b border-purple-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div className="flex items-center justify-between">
            {/* Logo */}
            <div className="text-center flex-1">
              <h1 className="text-4xl font-bold bg-gradient-to-r from-purple-600 to-pink-600 bg-clip-text text-transparent">
                🌺 Orchid Garden 🌺
              </h1>
              <p className="mt-2 text-gray-600 text-lg">
                Bộ sưu tập hoa lan đẹp nhất
              </p>
            </div>

            {/* Right Section - Cart & Auth */}
            <div className="absolute right-4 top-6 flex items-center space-x-4">
              {/* Cart Icon */}
              <button
                onClick={() => setIsCartOpen(true)}
                className="relative p-3 bg-gradient-to-r from-purple-100 to-pink-100 hover:from-purple-200 hover:to-pink-200 rounded-full transition-all duration-300"
              >
                <svg className="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                </svg>
                {cart.totalItems > 0 && (
                  <span className="absolute -top-2 -right-2 bg-red-500 text-white text-xs font-bold rounded-full h-6 w-6 flex items-center justify-center">
                    {cart.totalItems > 99 ? '99+' : cart.totalItems}
                  </span>
                )}
              </button>

              {/* Auth Section */}
              {isAuthenticated && user ? (
                <div className="relative">
                  <button
                    onClick={() => setShowUserMenu(!showUserMenu)}
                    className="flex items-center space-x-3 bg-gradient-to-r from-purple-100 to-pink-100 hover:from-purple-200 hover:to-pink-200 px-4 py-2 rounded-full transition-all duration-300"
                  >
                    <div className="w-8 h-8 bg-gradient-to-r from-purple-500 to-pink-500 rounded-full flex items-center justify-center">
                      <span className="text-white font-semibold text-sm">
                        {user?.accountName?.charAt(0)?.toUpperCase() || 'U'}
                      </span>
                    </div>
                    <div className="text-left">
                      <p className="text-sm font-medium text-gray-800">
                        Xin chào, {user?.accountName}
                      </p>
                      {role && (
                        <p className="text-xs text-purple-600">
                          {role}
                        </p>
                      )}
                    </div>
                    <svg
                      className={`w-4 h-4 text-gray-600 transform transition-transform ${showUserMenu ? 'rotate-180' : ''}`}
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                    </svg>
                  </button>

                  {/* User Menu Dropdown */}
                  {showUserMenu && (
                    <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border border-gray-200 py-2 z-[60]">
                      <div className="px-4 py-2 border-b border-gray-100">
                        <p className="text-sm font-medium text-gray-800">{user.accountName}</p>
                        <p className="text-xs text-gray-500">{user?.email || ''}</p>
                        {role && (
                          <p className="text-xs text-purple-600 font-medium mt-1">
                            Vai trò: {role}
                          </p>
                        )}
                      </div>

                      {/* Admin Dashboard Link */}
                      {role === 'Admin' && (
                        <Link
                          href="/admin"
                          onClick={() => setShowUserMenu(false)}
                          className="w-full text-left px-4 py-2 text-sm text-green-600 hover:bg-gradient-to-r hover:from-green-50 hover:to-green-100 hover:text-green-700 hover:font-medium transition-all duration-300 cursor-pointer transform hover:scale-105 flex items-center space-x-2"
                        >
                          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                          </svg>
                          <span>Quản trị viên</span>
                        </Link>
                      )}

                      {/* Order History Link */}
                      <button
                        onClick={() => {
                          setShowOrderHistory(true);
                          setShowUserMenu(false);
                        }}
                        className="w-full text-left px-4 py-2 text-sm text-blue-600 hover:bg-gradient-to-r hover:from-blue-50 hover:to-blue-100 hover:text-blue-700 hover:font-medium transition-all duration-300 cursor-pointer transform hover:scale-105 flex items-center space-x-2"
                      >
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
                        </svg>
                        <span>Lịch sử đơn hàng</span>
                      </button>

                      <button
                        onClick={(e) => {
                          e.preventDefault();
                          e.stopPropagation();

                          if (logout && typeof logout === 'function') {
                            logout().then(() => {
                              setShowUserMenu(false);
                            }).catch(err => {
                              console.error('❌ Direct logout call failed:', err);
                            });
                          } else {
                            console.error('❌ logout is not available or not a function:', logout);
                          }
                        }}
                        className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-gradient-to-r hover:from-red-50 hover:to-red-100 hover:text-red-700 hover:font-medium transition-all duration-300 cursor-pointer transform hover:scale-105 flex items-center space-x-2"
                        style={{
                          pointerEvents: 'auto',
                          zIndex: 1000,
                          position: 'relative'
                        }}
                      >
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                        </svg>
                        <span>Đăng xuất</span>
                      </button>
                    </div>
                  )}
                </div>
              ) : (
                <div className="flex space-x-3">
                  <button
                    onClick={() => setShowLoginModal(true)}
                    className="px-6 py-2 text-purple-600 border border-purple-600 rounded-full hover:bg-purple-50 transition-all duration-300"
                  >
                    Đăng nhập
                  </button>
                  <button
                    onClick={() => setShowRegisterModal(true)}
                    className="px-6 py-2 bg-gradient-to-r from-purple-500 to-pink-500 text-white rounded-full hover:from-purple-600 hover:to-pink-600 transition-all duration-300"
                  >
                    Đăng ký
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>
      </header>

      {/* Cart Component */}
      <Cart
        isOpen={isCartOpen}
        onCloseAction={() => setIsCartOpen(false)}
      />

      {/* Auth Modals */}
      <LoginModal
        isOpen={showLoginModal}
        onClose={() => setShowLoginModal(false)}
        onSwitchToRegister={handleSwitchToRegister}
      />
      <RegisterModal
        isOpen={showRegisterModal}
        onClose={() => setShowRegisterModal(false)}
        onSwitchToLogin={handleSwitchToLogin}
      />
      <OrderHistoryModal
        isOpen={showOrderHistory}
        onClose={() => setShowOrderHistory(false)}
      />
    </>
  );
}
