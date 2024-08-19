import React, { useEffect, useState } from 'react';
import { getAllRidesAdmin } from '../../services/rideService';
import { useNavigate } from 'react-router-dom';
import './AdminDashboardPage.css';

interface Driver {
  id: string;
  name: string;
  email: string;
}

interface Ride {
  id: string;
  startAddress: string;
  endAddress: string;
  price: number;
  driverTimeInSeconds: number;
  arrivalTimeInSeconds: number;
  status: number;
  driver?: Driver; // Informacije o vozaču su opcionalne
}

const AdminDashboardPage: React.FC = () => {
  const [rides, setRides] = useState<Ride[]>([]);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchRides = async () => {
      try {
        const ridesData = await getAllRidesAdmin();
        console.log('Rides Data:', ridesData); // Provera podataka u konzoli
        setRides(ridesData);
      } catch (error) {
        console.error('Failed to fetch rides:', error);
      }
    };

    fetchRides();
  }, []);

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    localStorage.removeItem('role');
    navigate('/login');
  };

  const handleUpdateProfile = () => {
    navigate('/update-profile');
  };

  const handleViewRatings = () => {
    navigate('/driver-ratings'); // Navigacija ka stranici sa ocenama vozača
  };

  const handleVerifyDrivers = () => {
    navigate('/verify-drivers'); // Navigacija ka stranici za verifikaciju vozača
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
        <button onClick={handleViewRatings} className="rating-button">Ratings</button>
        <button onClick={handleVerifyDrivers} className="verify-drivers-button">Verify Drivers</button>
        <button onClick={handleUpdateProfile} className="update-profile-button">Update Profile</button>
        <button onClick={handleLogout} className="logout-button">Logout</button>
      </div>
      <div className="dashboard-box">
        <h2 className="dashboard-title">All Rides</h2>
        <ul className="rides-list">
          {rides.map((ride: Ride) => (
            <li key={ride.id} className="ride-item">
              <div className="ride-item-header">
                <span className="ride-address">From: {ride.startAddress} To: {ride.endAddress}</span>
              </div>
              <div className="ride-details">
                <span className="ride-info">Driver Time: {ride.driverTimeInSeconds} seconds</span>
                <span className="ride-info">Arrival Time: {ride.arrivalTimeInSeconds} seconds</span>
                <span className="ride-info">Price: {ride.price} RSD</span>
                <span className={`ride-status ${getStatusClass(ride.status)}`}>
                  {ride.status === 0 && 'PENDING'}
                  {ride.status === 1 && 'CONFIRMED'}
                  {ride.status === 2 && 'FINISHED'}
                </span>
                {ride.driver ? (
                  <div className="driver-info">
                    <h4>Driver Information</h4>
                    <p>Name: {ride.driver.name || 'N/A'}</p><br />
                    <p>Email: {ride.driver.email || 'N/A'}</p>
                  </div>
                ) : (
                  <div className="driver-info">
                    <p>No driver assigned</p>
                  </div>
                )}
              </div>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
};

export default AdminDashboardPage;
