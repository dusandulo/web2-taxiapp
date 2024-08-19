import apiClient from './apiClient';

enum VerificationState {
  Verified = 'Verified',
  Unverified = 'Unverified',
  Blocked = 'Blocked',
}

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

export const blockDriver = async (driverId: string) => {
  const token = localStorage.getItem('token');
  
  const response = await apiClient.post(`/users/block/${driverId}`, {}, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return response.data;
};

export const unblockDriver = async (driverId: string) => {
  const token = localStorage.getItem('token');
  
  const response = await apiClient.post(`/users/unblock/${driverId}`, {}, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return response.data;
};

export const getUnverifiedDrivers = async () => {
    const token = localStorage.getItem('token');
    const response = await apiClient.get('/users/unverified-drivers', {
        headers: {
            Authorization: `Bearer ${token}`,
        },
    });
    return response.data;
};

export const verifyDriver = async (driverId: string) => {
    const token = localStorage.getItem('token');
    const response = await apiClient.post(`/users/verify/${driverId}`, {}, {
        headers: {
            Authorization: `Bearer ${token}`,
        },
    });
    return response.data;
};

export const getUserProfileById = async (userId: string) => {
  const token = localStorage.getItem('token');

  if (!token) {
    throw new Error('No token found in localStorage');
  }

  try {
    const response = await apiClient.get(`/users/${userId}`, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });

    if (response.status === 404) {
      throw new Error('User profile not found for the given ID');
    }

    const userProfile = response.data;

    // Mapiranje numeričkih vrednosti na stringove za VerificationState
    switch (userProfile.verificationState) {
      case 0:
        userProfile.verificationState = VerificationState.Verified;
        break;
      case 1:
        userProfile.verificationState = VerificationState.Unverified;
        break;
      case 3:
        userProfile.verificationState = VerificationState.Blocked;
        break;
      default:
        userProfile.verificationState = VerificationState.Unverified;
    }

    return userProfile;
  } catch (error) {
    console.error('Failed to fetch user profile:', error);
    throw error;
  }
};