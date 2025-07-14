// API Configuration
export const API_CONFIG = {
  BASE_URL: 'http://localhost:5001/api/v1',
  AUTH_URL: 'http://localhost:5001',
  ENDPOINTS: {
    SELECT_ORCHIDS: '/SelectOrchids',
    SELECT_CATEGORIES: '/SelectCategories',
    INSERT_ACCOUNT: '/InsertAccount',
    SELECT_ACCOUNT_PROFILE: '/SelectAccountProfile',
    SELECT_TOKEN: '/Auth/SelectToken',
    INSERT_ORDER: '/InsertOrder',
    PAYMENT_CALLBACK: '/PaymentOrderCallback',
    // Admin Orchid Management
    INSERT_ORCHID: '/InsertOrchid',
    UPDATE_ORCHID: '/UpdateOrchid',
    DELETE_ORCHID: '/DeleteOrchid',
    // Admin Category Management
    INSERT_CATEGORY: '/InsertCategory',
    UPDATE_CATEGORY: '/UpdateCategory',
    DELETE_CATEGORY: '/DeleteCategory',
    // Admin Analytics
    ADMIN_BEST_SELLING: '/admin/SelectAdminBestSellingOrchids',
    ADMIN_ORDERS: '/admin/SelectAdminOrders',
    ADMIN_STATISTICS: '/admin/SelectAdminOrdersStatistics'
  },
  AUTH_ENDPOINTS: {
    TOKEN: '/connect/token',
    LOGOUT: '/connect/logout'
  }
};

// Helper function to build complete API URLs
export const buildApiUrl = (endpoint: string, params?: Record<string, string | number>) => {
  let url = `${API_CONFIG.BASE_URL}${endpoint}`;

  if (params) {
    const searchParams = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
      searchParams.append(key, value.toString());
    });
    url += `?${searchParams.toString()}`;
  }

  return url;
};

// Helper function for auth URLs
export const buildAuthUrl = (endpoint: string) => {
  return `${API_CONFIG.AUTH_URL}${endpoint}`;
};
