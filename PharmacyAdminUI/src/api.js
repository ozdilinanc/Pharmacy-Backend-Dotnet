import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5144/api', // ASP.NET Core default port. Change if different.
  headers: {
    'Content-Type': 'application/json',
  },
});

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

export const getPharmacies = async () => {
  const response = await api.get('/pharmacies');
  return response.data;
};

export const getSuggestions = async (unmatchedPharmacyId) => {
  const response = await api.get(`/admin/unmatched/${unmatchedPharmacyId}/suggestions`);
  return response.data;
};

export default api;
