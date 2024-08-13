import apiClient from './apiClient';

export const getUserProfile = async () => {
    const token = localStorage.getItem('token');
    
    if (!token) {
      throw new Error('No token found in localStorage');
    }
  
    try {
      // Decode JWT token to extract user email
      const base64Url = token.split('.')[1];
      if (!base64Url) {
        throw new Error('Invalid token format');
      }
  
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const decodedData = JSON.parse(window.atob(base64));
  
      const email = decodedData.email;
  
      if (!email) {
        throw new Error('Email not found in token');
      }
      const response = await apiClient.get(`/users/email/${email}`, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });
  
      if (response.status === 404) {
        throw new Error('User profile not found for the given email');
      }
      
      return response.data;
    } catch (error) {
      console.error('Failed to decode token or fetch user profile:', error);
      throw error;
    }
  };


export const updateUserProfile = async (userData: {
  id: string;
  userName: string;
  email: string;
  password?: string;
  name: string;
  lastName: string;
  address: string;
  birthday: string;
  role: string;
}) => {
  const token = localStorage.getItem('token');
  const response = await apiClient.put('/users/update', userData, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });
  return response.data;
};
