'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import AdminDashboard from '@/components/AdminDashboard';

export default function AdminPage() {
  const { isAuthenticated, role, authReady } = useAuth();
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (authReady) {
      if (!isAuthenticated) {
        router.push('/');
        return;
      }

      if (role !== 'Admin') {
        router.push('/');
        return;
      }

      setIsLoading(false);
    }
  }, [authReady, isAuthenticated, role, router]);

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-green-500 mx-auto"></div>
          <p className="mt-4 text-gray-600">Đang kiểm tra quyền truy cập...</p>
        </div>
      </div>
    );
  }

  return <AdminDashboard />;
}
