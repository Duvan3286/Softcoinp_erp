import axios from 'axios';

const API_URL = process.env.NEXT_PUBLIC_API_URL || '/api';
const isSameOrigin = API_URL.startsWith('/');

const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: !isSameOrigin,
});

const getSubdomain = () => {
  if (typeof window === 'undefined') return process.env.NEXT_PUBLIC_TENANT_ID ?? '';
  const hostname = window.location.hostname;
  const parts = hostname.split('.');
  if (parts.length > 2) return parts[0];
  if (parts.length === 2 && parts[1].includes('localhost')) return parts[0];
  return process.env.NEXT_PUBLIC_TENANT_ID ?? '';
};

const setAuthCookie = (token: string) => {
  if (typeof window === 'undefined' || isSameOrigin) return;
  document.cookie = `auth_token=${token}; path=/; max-age=86400; samesite=lax`;
};

const clearAuthCookie = () => {
  if (typeof window === 'undefined' || isSameOrigin) return;
  document.cookie = 'auth_token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT; samesite=lax';
};

apiClient.interceptors.request.use(
  (config) => {
    const tenantId = getSubdomain();
    if (tenantId) {
      config.headers['X-Tenant-Id'] = tenantId;
    }
    if (config.method && config.method !== 'get' && config.method !== 'head') {
      config.headers['X-Requested-With'] = 'XMLHttpRequest';
    }
    // Modo cross-origen: enviar token desde sessionStorage como Authorization header
    if (!isSameOrigin) {
      const token = sessionStorage.getItem('auth_token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    }
    // Modo mismo origen: la cookie httpOnly se envía automáticamente
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

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        if (!isSameOrigin) {
          const refreshToken = sessionStorage.getItem('refresh_token');
          if (refreshToken) {
            const tenantId = getSubdomain();
            const headers: Record<string, string> = {};
            if (tenantId) {
              headers['X-Tenant-Id'] = tenantId;
            }
            const response = await axios.post(`${API_URL}/auth/refresh`, {
              refreshToken,
            }, { headers, withCredentials: true });

            const { token } = response.data;
            sessionStorage.setItem('auth_token', token);
            setAuthCookie(token);

            originalRequest.headers.Authorization = `Bearer ${token}`;
            return apiClient(originalRequest);
          }
        } else {
          // Modo mismo origen: el refresh usa la cookie httpOnly
          const tenantId = getSubdomain();
          const headers: Record<string, string> = {};
          if (tenantId) {
            headers['X-Tenant-Id'] = tenantId;
          }
          await axios.post('/api/auth/refresh', {}, { headers });

          return apiClient(originalRequest);
        }
      } catch {
        if (!isSameOrigin) {
          sessionStorage.removeItem('auth_token');
          sessionStorage.removeItem('refresh_token');
          clearAuthCookie();
        }
        if (typeof window !== 'undefined' && window.location.pathname !== '/login') {
          window.location.href = '/login';
        }
      }
    }

    return Promise.reject(error);
  }
);

export { setAuthCookie, clearAuthCookie };
export default apiClient;
