'use client';

import { useEffect, useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import AdminDashboard from '@/components/AdminDashboard';
import { API_CONFIG, buildApiUrl } from '@/config/api';

export default function AdminPage() {
  const { accessToken, authReady } = useAuth();
  const router = useRouter();
  const [isAdmin, setIsAdmin] = useState<boolean | null>(null);
  const [loading, setLoading] = useState(true);
  const [hasChecked, setHasChecked] = useState(false);

  const checkAdminRole = useCallback(async () => {
    // Don't check if auth is still loading or we already checked
    if (!authReady || hasChecked) return;

    // If no token after auth loading is complete, redirect
    if (!accessToken) {
      console.log('No access token found after auth loading completed, redirecting to home');
      setIsAdmin(false);
      setLoading(false);
      router.push('/');
      return;
    }

    try {
      console.log('Checking admin role with token:', accessToken.substring(0, 20) + '...');
      setHasChecked(true);

      // Method 1: Try SelectToken API
      let response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.SELECT_TOKEN), {
        headers: { 'Authorization': `Bearer ${accessToken}` }
      });

      console.log('SelectToken Response status:', response.status);

      if (response.ok) {
        const data = await response.json();
        console.log('SelectToken Response data:', data);

        // Fix: Check for roleName in data.response and fallback to other fields
        let userRole = undefined;
        if (data.response) {
          userRole = data.response.roleName || data.response.role || data.response.userRole || data.response.Role || data.response.UserRole;
        }
        if (!userRole) {
          userRole = data.roleName || data.role || data.userRole || data.Role || data.UserRole;
        }

        console.log('User role found:', userRole);

        if (userRole && String(userRole).toLowerCase() === 'admin') {
          console.log('User is admin, granting access');
          setIsAdmin(true);
          setLoading(false);
          return;
        }
      } else {
        console.log('SelectToken Response not OK:', response.status, response.statusText);
      }

      // Method 2: Try SelectAccountProfile API as fallback
      console.log('Trying SelectAccountProfile API as fallback...');
      response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.SELECT_ACCOUNT_PROFILE), {
        headers: { 'Authorization': `Bearer ${accessToken}` }
      });

      console.log('SelectAccountProfile Response status:', response.status);

      if (response.ok) {
        const profileData = await response.json();
        console.log('SelectAccountProfile Response data:', profileData);

        // Check role in profile data
        const profileRole = profileData.response?.roleName ||
                           profileData.response?.role ||
                           profileData.response?.userRole ||
                           profileData.response?.Role ||
                           profileData.response?.UserRole ||
                           profileData.roleName ||
                           profileData.role;

        console.log('Profile role found:', profileRole);

        if (profileRole && profileRole.toLowerCase() === 'admin') {
          console.log('User is admin (via profile), granting access');
          setIsAdmin(true);
          setLoading(false);
          return;
        }
      }

      // Method 3: Parse JWT token directly as last resort
      console.log('Trying to parse JWT token directly...');
      try {
        const payload = JSON.parse(atob(accessToken.split('.')[1]));
        console.log('JWT payload:', payload);

        const jwtRole = payload.role || payload.Role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        console.log('JWT role found:', jwtRole);

        if (jwtRole && jwtRole.toLowerCase() === 'admin') {
          console.log('User is admin (via JWT), granting access');
          setIsAdmin(true);
          setLoading(false);
          return;
        }
      } catch (jwtError) {
        console.log('Error parsing JWT:', jwtError);
      }

      // If all methods fail, deny access
      console.log('User is not admin, denying access');
      setIsAdmin(false);
      setLoading(false);
      router.push('/');

    } catch (error) {
      console.error('Error checking admin role:', error);
      setIsAdmin(false);
      setLoading(false);
      router.push('/');
    }
  }, [accessToken, authReady, hasChecked, router]);

  useEffect(() => {
    checkAdminRole();
  }, [checkAdminRole]);

  // Show loading while auth is loading or we're checking admin status
  if (!authReady || loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-green-600 mx-auto"></div>
          <p className="mt-4 text-lg text-gray-600">
            {!authReady ? 'Loading authentication...' : 'Checking admin permissions...'}
          </p>
        </div>
      </div>
    );
  }

  if (!isAdmin) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <h1 className="text-4xl font-bold text-red-600 mb-4">Access Denied</h1>
          <p className="text-lg text-gray-600 mb-6">You don&apos;t have permission to access this page.</p>
          <button
            onClick={() => router.push('/')}
            className="bg-green-600 text-white px-6 py-3 rounded-lg hover:bg-green-700"
          >
            Go to Home
          </button>
        </div>
      </div>
    );
  }

  return <AdminDashboard />;
}
