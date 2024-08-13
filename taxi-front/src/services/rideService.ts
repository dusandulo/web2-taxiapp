import apiClient from './apiClient';

export const getAllRides = async () => {
  const token = localStorage.getItem('token');
  
  const response = await apiClient.get('/ride/getallrides', {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return response.data;
};

export const createRide = async (rideData: {
  startAddress: string;
  endAddress: string;
  price: number;
  driverTimeInSeconds: number;
}) => {
  const token = localStorage.getItem('token');
  
  const response = await apiClient.post('/ride/createride', rideData, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return response.data;
};

export const estimateRide = async (startAddress: string, endAddress: string) => {
  const token = localStorage.getItem('token');
  
  const response = await apiClient.post('/ride/estimate', { startAddress, endAddress }, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return response.data;
};

export const getRideById = async (rideId: string) => {
  const token = localStorage.getItem('token');

  const response = await apiClient.get(`/ride/${rideId}`, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return response.data;
};

export const confirmRide = async (rideId: string) => {
  const token = localStorage.getItem('token');

  const response = await apiClient.post('/ride/confirm', { rideId }, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return response.data;
};

export const getPendingRides = async () => {
  const token = localStorage.getItem('token');
  
  const response = await apiClient.get('/ride/pendingrides', {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return response.data;
};