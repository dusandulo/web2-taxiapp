import apiClient from './apiClient';

export const login = async (email: string, password: string) => {
  return apiClient.post('/users/login', { email, password });
};