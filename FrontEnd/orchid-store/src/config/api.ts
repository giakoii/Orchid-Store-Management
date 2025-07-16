// API Configuration
export const API_CONFIG = {
  BASE_URL: 'http://localhost:5001',
  AUTH_URL: 'http://localhost:5001',
  ENDPOINTS: {
    SELECT_ORCHIDS: '/api/v1/SelectOrchids',
    SELECT_CATEGORIES: '/api/v1/SelectCategories',
    INSERT_ACCOUNT: '/api/v1/InsertAccount',
    SELECT_ACCOUNT_PROFILE: '/api/v1/SelectAccountProfile',
    SELECT_TOKEN: '/api/v1/Auth/SelectToken',
    INSERT_ORDER: '/api/v1/InsertOrder',
    PAYMENT_CALLBACK: '/api/v1/PaymentOrderCallback',
    // Admin Orchid Management
    INSERT_ORCHID: '/api/v1/InsertOrchid',
    UPDATE_ORCHID: '/api/v1/UpdateOrchid',
    DELETE_ORCHID: '/api/v1/DeleteOrchid',
    // Admin Category Management
    INSERT_CATEGORY: '/api/v1/InsertCategory',
    UPDATE_CATEGORY: '/api/v1/UpdateCategory',
    DELETE_CATEGORY: '/api/v1/DeleteCategory',
    // Admin Analytics
    ADMIN_BEST_SELLING: '/api/v1/admin/SelectAdminBestSellingOrchids',
    ADMIN_ORDERS: '/api/v1/admin/SelectAdminOrders',
    ADMIN_STATISTICS: '/api/v1/admin/SelectAdminOrdersStatistics',
    // Order History
    SELECT_ORDERS: '/api/v1/SelectOrders',
    SELECT_ORDER: '/api/v1/SelectOrder',
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
