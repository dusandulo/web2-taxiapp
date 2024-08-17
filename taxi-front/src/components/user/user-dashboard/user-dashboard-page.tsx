import React, { useEffect, useState } from 'react';
import { getAllRides } from '../../../services/rideService';
import { useNavigate } from 'react-router-dom';
import './UserDashboardPage.css';

interface Ride {
  id: string;
  startAddress: string;
  endAddress: string;
  price: number;
  driverTimeInSeconds: number;
  arrivalTimeInSeconds: number;
  status: number;
}

const UserDashboardPage: React.FC = () => {
  const [rides, setRides] = useState<Ride[]>([]);
  const userId = localStorage.getItem('userId');
  const navigate = useNavigate();

  useEffect(() => {
    const fetchRides = async () => {
      try {
        const ridesData = await getAllRides(userId!);
        setRides(ridesData);
      } catch (error) {
        console.error('Failed to fetch rides:', error);
      }
    };

    fetchRides();
  }, [userId]);

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    localStorage.removeItem('role');
    navigate('/login');
  };

  const handleUpdateProfile = () => {
    navigate('/update-profile');
  };

  const getStatusClass = (status: number) => {
    switch (status) {
      case 0:
        return 'status-pending';
      case 1:
        return 'status-confirmed';
      case 2:
        return 'status-finished';
      default:
        return '';
    }
  };

  return (
    <div className="dashboard-container">
      <div className="navigation-buttons">
        <button onClick={() => navigate('/create-ride')} className="create-ride-button">Create Ride</button>
        <button onClick={handleUpdateProfile} className="update-profile-button">Update Profile</button>
        <button onClick={handleLogout} className="logout-button">Logout</button>
      </div>
      <div className="dashboard-box">
        <h2 className="dashboard-title">Your Rides</h2>
        <ul className="rides-list">
          {rides.map((ride: Ride) => (
            <li key={ride.id} className="ride-item">
              <div className="ride-item-header">
                <span className="ride-address">{ride.startAddress} to {ride.endAddress}</span>
                <span className="ride-info">Price: ${ride.price}</span>
              </div>
              <div className="ride-details">
                <span className="ride-info">Driver Time: {ride.driverTimeInSeconds} seconds</span>
                <span className="ride-info">Arrival Time: {ride.arrivalTimeInSeconds} seconds</span>
                <span className={`ride-status ${getStatusClass(ride.status)}`}>
                  {ride.status === 0 && 'PENDING'}
                  {ride.status === 1 && 'CONFIRMED'}
                  {ride.status === 2 && 'FINISHED'}
                </span>
              </div>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
};

export default UserDashboardPage;
