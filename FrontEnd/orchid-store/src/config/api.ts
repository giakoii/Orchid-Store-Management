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
    INSERT_ORDER: '/InsertOrder'
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
