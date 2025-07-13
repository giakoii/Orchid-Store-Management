// Auth types
export interface RegisterRequest {
  accountName: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  username: string;
  password: string;
  grant_type: 'password';
}

export interface RefreshTokenRequest {
  grant_type: 'refresh_token';
  refresh_token: string;
}

export interface LoginResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
  scope: string;
  refresh_token: string;
}

export interface UserProfile {
  accountId: number;
  accountName: string;
  email: string;
}

export interface UserRole {
  roleName: string;
}

export interface ApiResponse<T> {
  response: T;
  success: boolean;
  messageId: string;
  message: string;
  detailErrorList: any;
}

export interface AuthState {
  isAuthenticated: boolean;
  user: UserProfile | null;
  role: string | null;
  accessToken: string | null;
  refreshToken: string | null;
}
