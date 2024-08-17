import apiClient from './apiClient';

export const login = async (email: string, password: string) => {
  const response = await apiClient.post('/users/login', { email, password });
  
  if (!response.data || !response.data.token || !response.data.userId || !response.data.role) {
    throw new Error('Invalid login response');
  }

  return response;
};