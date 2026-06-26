import axios from 'axios';

const apiClient = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

const getSubdomain = () => {
  if (typeof window === 'undefined') return process.env.NEXT_PUBLIC_TENANT_ID ?? '';
  const hostname = window.location.hostname;
  const parts = hostname.split('.');
  if (parts.length > 2) return parts[0];
  if (parts.length === 2 && parts[1].includes('localhost')) return parts[0];
  return process.env.NEXT_PUBLIC_TENANT_ID ?? '';
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
        const tenantId = getSubdomain();
        const headers: Record<string, string> = {};
        if (tenantId) {
          headers['X-Tenant-Id'] = tenantId;
        }
        await axios.post('/api/auth/refresh', {}, { headers });

        return apiClient(originalRequest);
      } catch {
        if (typeof window !== 'undefined') {
          window.location.href = '/login';
        }
      }
    }

    return Promise.reject(error);
  }
);

export default apiClient;
