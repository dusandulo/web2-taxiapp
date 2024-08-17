import apiClient from './apiClient';

export const register = async (formData: FormData) => {
  return apiClient.post('/users/register', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });
};
