import axios from 'axios';

const apiClient = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5005/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Helper to get subdomain on the client
const getSubdomain = () => {
  if (typeof window === 'undefined') return '';
  const hostname = window.location.hostname;
  const parts = hostname.split('.');
  if (parts.length > 2) return parts[0];
  if (parts.length === 2 && parts[1].includes('localhost')) return parts[0];
  return '';
};

apiClient.interceptors.request.use(
  (config) => {
    // Add Tenant ID
    const tenantId = getSubdomain();
    if (tenantId) {
      config.headers['X-Tenant-Id'] = tenantId;
    }

    // Add Authorization header
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('auth_token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Handle 401 Unauthorized (expired token)
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = localStorage.getItem('refresh_token');
        if (refreshToken) {
          // Attempt to refresh token
          const response = await axios.post(`${process.env.NEXT_PUBLIC_API_URL}/auth/refresh`, {
            refreshToken,
          });

          const { token } = response.data;
          localStorage.setItem('auth_token', token);

          originalRequest.headers.Authorization = `Bearer ${token}`;
          return apiClient(originalRequest);
        }
      } catch {
        // Refresh token failed or expired
        localStorage.removeItem('auth_token');
        localStorage.removeItem('refresh_token');
        if (typeof window !== 'undefined') {
          window.location.href = '/login';
        }
      }
    }

    return Promise.reject(error);
  }
);

export default apiClient;
