'use client';

import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
  useCallback,
} from 'react';
import {
  AuthState,
  UserProfile,
  LoginRequest,
  RegisterRequest,
  LoginResponse,
  ApiResponse,
  UserRole,
} from '@/types/auth';
import { API_CONFIG, buildApiUrl, buildAuthUrl } from '@/config/api';

interface AuthContextType extends AuthState {
  authReady: boolean;
  login: (
      credentials: Omit<LoginRequest, 'grant_type'>
  ) => Promise<{ success: boolean; message: string }>;
  register: (
      data: RegisterRequest
  ) => Promise<{ success: boolean; message: string }>;
  logout: () => Promise<void>;
  refreshAccessToken: () => Promise<boolean>;
  fetchUserProfile: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const STORAGE_KEYS = {
  ACCESS_TOKEN: 'access_token',
  REFRESH_TOKEN: 'refresh_token',
  USER_PROFILE: 'user_profile',
  USER_ROLE: 'user_role',
};

export function AuthProvider({ children }: { children: ReactNode }) {
  const [authState, setAuthState] = useState<AuthState>({
    isAuthenticated: false,
    user: null,
    role: null,
    accessToken: null,
    refreshToken: null,
  });

  const [authReady, setAuthReady] = useState(false);
  const [isHydrated, setIsHydrated] = useState(false);

  // ✅ Step 1: Set hydration flag
  useEffect(() => {
    setIsHydrated(true);
  }, []);

  // ✅ Step 2: Init auth state from localStorage
  useEffect(() => {
    if (!isHydrated) return;

    const initializeAuth = () => {
      try {
        const accessToken = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
        const refreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
        const userProfile = localStorage.getItem(STORAGE_KEYS.USER_PROFILE);
        const userRole = localStorage.getItem(STORAGE_KEYS.USER_ROLE);

        if (accessToken && refreshToken && userProfile) {
          const parsedUser = JSON.parse(userProfile);

          setAuthState({
            isAuthenticated: true,
            user: parsedUser,
            role: userRole,
            accessToken,
            refreshToken,
          });
        } else {
          setAuthState({
            isAuthenticated: false,
            user: null,
            role: null,
            accessToken: null,
            refreshToken: null,
          });
        }
      } catch (error) {
        console.error("❌ [Auth Init] Error parsing localStorage:", error);
        Object.values(STORAGE_KEYS).forEach((key) => localStorage.removeItem(key));

        setAuthState({
          isAuthenticated: false,
          user: null,
          role: null,
          accessToken: null,
          refreshToken: null,
        });
      } finally {
        setAuthReady(true);
      }
    };

    initializeAuth();

    // Sync across tabs (e.g. when logout/login happens in another tab)
    const handleStorageChange = (e: StorageEvent) => {
      if (Object.values(STORAGE_KEYS).includes(e.key as any)) {
        initializeAuth();
      }
    };

    window.addEventListener("storage", handleStorageChange);
    return () => window.removeEventListener("storage", handleStorageChange);
  }, [isHydrated]);
  const login = async (
      credentials: Omit<LoginRequest, 'grant_type'>
  ): Promise<{ success: boolean; message: string }> => {
    try {
      const formData = new URLSearchParams();
      formData.append('grant_type', 'password');
      formData.append('username', credentials.username);
      formData.append('password', credentials.password);

      const response = await fetch(buildAuthUrl(API_CONFIG.AUTH_ENDPOINTS.TOKEN), {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: formData,
      });

      if (response.ok) {
        const loginData: LoginResponse = await response.json();

        localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, loginData.access_token);
        localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, loginData.refresh_token);

        setAuthState((prev) => ({
          ...prev,
          accessToken: loginData.access_token,
          refreshToken: loginData.refresh_token,
        }));

        await fetchUserProfileAndRole(loginData.access_token);

        return { success: true, message: 'Đăng nhập thành công!' };
      } else {
        const errorData = await response.json();
        return { success: false, message: errorData.error_description || 'Đăng nhập thất bại' };
      }
    } catch (error) {
      console.error('Login error:', error);
      return { success: false, message: 'Lỗi kết nối. Vui lòng thử lại.' };
    }
  };

  const register = async (
      data: RegisterRequest
  ): Promise<{ success: boolean; message: string }> => {
    try {
      const response = await fetch(buildApiUrl(API_CONFIG.ENDPOINTS.INSERT_ACCOUNT), {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });

      const result: ApiResponse<null> = await response.json();

      if (result.success) {
        return { success: true, message: 'Đăng ký thành công! Vui lòng đăng nhập.' };
      } else {
        return { success: false, message: result.message || 'Đăng ký thất bại' };
      }
    } catch (error) {
      console.error('Register error:', error);
      return { success: false, message: 'Lỗi kết nối. Vui lòng thử lại.' };
    }
  };

  const fetchUserProfileAndRole = async (token: string) => {
    try {
      const [profileRes, roleRes] = await Promise.all([
        fetch(buildApiUrl(API_CONFIG.ENDPOINTS.SELECT_ACCOUNT_PROFILE), {
          headers: { Authorization: `Bearer ${token}` },
        }),
        fetch(buildApiUrl(API_CONFIG.ENDPOINTS.SELECT_TOKEN), {
          headers: { Authorization: `Bearer ${token}` },
        }),
      ]);

      if (profileRes.ok && roleRes.ok) {
        const profileData: ApiResponse<UserProfile> = await profileRes.json();
        const roleData: ApiResponse<UserRole> = await roleRes.json();

        if (profileData.success && roleData.success) {
          const user = profileData.response;
          const role = roleData.response.roleName;

          localStorage.setItem(STORAGE_KEYS.USER_PROFILE, JSON.stringify(user));
          localStorage.setItem(STORAGE_KEYS.USER_ROLE, role);

          setAuthState((prev) => ({
            ...prev,
            isAuthenticated: true,
            user,
            role,
          }));
        }
      }
    } catch (error) {
      console.error('Error fetching user data:', error);
    }
  };

  const fetchUserProfile = async () => {
    if (authState.accessToken) {
      await fetchUserProfileAndRole(authState.accessToken);
    }
  };

  const refreshAccessToken = async (): Promise<boolean> => {
    try {
      if (!authState.refreshToken) return false;

      const formData = new URLSearchParams();
      formData.append('grant_type', 'refresh_token');
      formData.append('refresh_token', authState.refreshToken);

      const response = await fetch(buildAuthUrl(API_CONFIG.AUTH_ENDPOINTS.TOKEN), {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: formData,
      });

      if (response.ok) {
        const loginData: LoginResponse = await response.json();

        localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, loginData.access_token);
        localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, loginData.refresh_token);

        setAuthState((prev) => ({
          ...prev,
          accessToken: loginData.access_token,
          refreshToken: loginData.refresh_token,
        }));

        return true;
      } else {
        logout();
        return false;
      }
    } catch (error) {
      console.error('Refresh token error:', error);
      logout();
      return false;
    }
  };

  const logout = useCallback(async () => {
    const token = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);

    try {
      if (token) {
        const response = await fetch(buildAuthUrl(API_CONFIG.AUTH_ENDPOINTS.LOGOUT), {
          method: 'POST',
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            Authorization: `Bearer ${token}`,
          },
        });

        if (!response.ok) {
          console.warn('⚠️ Logout API failed:', response.statusText);
        }
      }
    } catch (error) {
      console.error('❌ Logout API error:', error);
    }

    // Clear localStorage
    Object.values(STORAGE_KEYS).forEach((key) => localStorage.removeItem(key));

    // Reset state
    setAuthState({
      isAuthenticated: false,
      user: null,
      role: null,
      accessToken: null,
      refreshToken: null,
    });

    // Dispatch storage event to sync across tabs
    window.dispatchEvent(new StorageEvent('storage', {
      key: STORAGE_KEYS.ACCESS_TOKEN,
      newValue: null,
      oldValue: token,
      storageArea: localStorage
    }));
  }, []);
  if (!isHydrated) return null;

  return (
      <AuthContext.Provider
          value={{
            ...authState,
            authReady,
            login,
            register,
            logout,
            refreshAccessToken,
            fetchUserProfile,
          }}
      >
        {children}
      </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};