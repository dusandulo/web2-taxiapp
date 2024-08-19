import apiClient from './apiClient';

export const getAllRides = async (userId: string) => {
  const token = localStorage.getItem('token');
  
  const response = await apiClient.get(`/ride/getallrides?userId=${userId}`, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return response.data;
};

export const getAllRidesAdmin = async () => {
  const token = localStorage.getItem('token');
  
  const response = await apiClient.get('/ride/getallridesadmin', {
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
  arrivalTimeInSeconds: number;
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
  const driverId = localStorage.getItem('userId'); 

  const response = await apiClient.post('/ride/confirm', { rideId, driverId }, {
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

export const finishRide = async (rideId: string, rideTimeInSeconds: number) => {
  const token = localStorage.getItem('token');
  
  const response = await apiClient.post(`/ride/${rideId}/finish`, 
    { rideId, rideTimeInSeconds }, 
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  return response.data;
};