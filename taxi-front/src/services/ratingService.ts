import apiClient from './apiClient';

export const addRating = async (ratingData: {
  rideId: string;
  driverId: string;
  passengerId: string;
  ratingValue: number;
  comment: string;
}) => {
  const token = localStorage.getItem('token');
  
  const response = await apiClient.post('/ratings/addrating', ratingData, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return response.data;
};

export const getRatingsForDriver = async (driverId: string) => {
  const token = localStorage.getItem('token');

  const response = await apiClient.get(`/ratings/getratings/${driverId}`, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return response.data;
};

export const getDriverRatings = async () => {
  const token = localStorage.getItem('token');
  
  const response = await apiClient.get('/ratings/getaverageratings', {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return response.data;
};

export const blockDriver = async (driverId: string) => {
  const token = localStorage.getItem('token');
  
  const response = await apiClient.post(`/ride/block/${driverId}`, {}, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return response.data;
};
