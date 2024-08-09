import axios from 'axios';

export const getRides = async (token: string) => {
  return axios.get('/api/rides', {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });
};