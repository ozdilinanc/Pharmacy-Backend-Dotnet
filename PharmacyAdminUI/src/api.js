import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5144/api', // ASP.NET Core default port. Change if different.
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && (error.response.status === 401 || error.response.status === 403)) {
      if (error.config.url !== '/auth/login') {
        localStorage.removeItem('token');
        window.location.reload();
      }
    }
    return Promise.reject(error);
  }
);

export const login = async (email, password) => {
  const response = await api.post('/auth/login', { email, password });
  return response.data;
};

export const getUnmatchedPharmacies = async () => {
  const response = await api.get('/admin/unmatched');
  return response.data;
};

export const matchPharmacy = async (unmatchedPharmacyId, realPharmacyId) => {
  const response = await api.post('/admin/match', {
    unmatchedPharmacyId,
    realPharmacyId,
  });
  return response.data;
};

export const deleteUnmatchedPharmacy = async (id) => {
  const response = await api.delete(`/admin/unmatched/${id}`);
  return response.data;
};

export const approveAsNewPharmacy = async (id, data = {}) => {
  const response = await api.post(`/admin/unmatched/${id}/approve`, data);
  return response.data;
};

export const getPharmacies = async () => {
  const response = await api.get('/pharmacies');
  return response.data;
};

export const getSuggestions = async (unmatchedPharmacyId) => {
  const response = await api.get(`/admin/unmatched/${unmatchedPharmacyId}/suggestions`);
  return response.data;
};

export const createPharmacy = async (pharmacyData) => {
  const response = await api.post('/pharmacies', pharmacyData);
  return response.data;
};

export const changeRole = async (email, newRole) => {
  const response = await api.put('/superadmin/change-role', { email, newRole });
  return response.data;
};

export default api;
