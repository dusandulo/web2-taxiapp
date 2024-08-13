import apiClient from './apiClient';

export const register = async (userData: {
  userName: string;
  email: string;
  password: string;
  name: string;
  lastName: string;
  address: string;
  birthday: string;
  role: number;
}) => {
  return apiClient.post('/users/register', userData);
};
