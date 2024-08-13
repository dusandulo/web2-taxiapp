import React, { useEffect, useState } from 'react';
import { getAllRides, getPendingRides, confirmRide } from '../../services/rideService';
import { Link } from 'react-router-dom';
import './DashboardPage.css';

const Dashboard: React.FC = () => {
  const [rides, setRides] = useState([]);
  const [pendingRides, setPendingRides] = useState([]);

  useEffect(() => {
    const fetchRides = async () => {
      try {
        const ridesData = await getAllRides();
        setRides(ridesData);

        const pendingRidesData = await getPendingRides();
        setPendingRides(pendingRidesData);
      } catch (error) {
        console.error('Failed to fetch rides:', error);
      }
    };

    fetchRides();
  }, []);

  const handleAcceptRide = async (rideId: string) => {
    try {
      await confirmRide(rideId);
      const updatedRides = await getAllRides();
      setRides(updatedRides);

      const pendingRidesData = await getPendingRides();
      setPendingRides(pendingRidesData);
    } catch (error) {
      console.error('Failed to accept ride:', error);
    }
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
      <Link to="/update-profile" className="update-profile-link">Update Profile</Link>
      <div className="dashboard-box">
        <h2 className="dashboard-title">Your Rides</h2>
        <ul className="rides-list">
          {rides.map((ride: any) => (
            <li key={ride.id} className="ride-item">
              <div className="ride-item-header">
                <span className="ride-address">{ride.startAddress} to {ride.endAddress}</span>
                <span className="ride-info">Price: ${ride.price}</span>
              </div>
              <div className="ride-details">
                <span className="ride-info">Driver Time: {Math.floor(ride.driverTimeInSeconds / 60)} mins</span>
                <span className="ride-info">Arrival Time: {Math.floor(ride.arrivalTimeInSeconds / 60)} mins</span>
                <span className={`ride-status ${getStatusClass(ride.status)}`}>
                  {ride.status === 0 && 'PENDING'}
                  {ride.status === 1 && 'CONFIRMED'}
                  {ride.status === 2 && 'FINISHED'}
                </span>
              </div>
            </li>
          ))}
        </ul>

        {pendingRides.length > 0 && (
          <>
            <h2 className="dashboard-title">Pending Rides</h2>
            <ul className="rides-list">
              {pendingRides.map((ride: any) => (
                <li key={ride.id} className="ride-item">
                  <div className="ride-item-header">
                    <span className="ride-address">{ride.startAddress} to {ride.endAddress}</span>
                    <button onClick={() => handleAcceptRide(ride.id)} className="accept-ride-button">Accept Ride</button>
                  </div>
                  <div className="ride-details">
                    <span className="ride-info">Price: ${ride.price}</span>
                    <span className="ride-info">Driver Time: {Math.floor(ride.driverTimeInSeconds / 60)} mins</span>
                    <span className={`ride-status ${getStatusClass(ride.status)}`}>
                      {ride.status === 0 && 'PENDING'}
                      {ride.status === 1 && 'CONFIRMED'}
                      {ride.status === 2 && 'FINISHED'}
                    </span>
                  </div>
                </li>
              ))}
            </ul>
          </>
        )}
      </div>
    </div>
  );
};

export default Dashboard;
