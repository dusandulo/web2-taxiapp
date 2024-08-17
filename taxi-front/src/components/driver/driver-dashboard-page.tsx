import React, { useEffect, useState } from 'react';
import { getPendingRides, confirmRide } from '../../services/rideService';
import { useNavigate } from 'react-router-dom';
import hubConnection, { startConnection, subscribeToNewRides, subscribeToRideConfirmation } from '../../services/signalRService';
import './DriverDashboardPage.css';

interface Ride {
  id: string;
  startAddress: string;
  endAddress: string;
  price: number;
  driverTimeInSeconds: number;
  arrivalTimeInSeconds: number;
  status: number;
}

const DriverDashboardPage: React.FC = () => {
  const [pendingRides, setPendingRides] = useState<Ride[]>([]);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchPendingRides = async () => {
      try {
        const pendingRidesData = await getPendingRides();
        setPendingRides(pendingRidesData);
      } catch (error) {
        console.error('Failed to fetch pending rides:', error);
      }
    };

    fetchPendingRides();

    startConnection().then(() => {
      const joinGroup = async () => {
        try {
          await hubConnection.invoke('JoinGroup', 'Drivers');
          console.log('Pridružio se grupi Drivers');
        } catch (error) {
          console.error('Greška prilikom pridruživanja grupi Drivers:', error);
        }
      };

      joinGroup();

      subscribeToNewRides((newRide: Ride) => {
        setPendingRides((prevPendingRides) => [...prevPendingRides, newRide]);
      });

      subscribeToRideConfirmation((confirmedRide: Ride) => {
        setPendingRides((prevPendingRides) =>
          prevPendingRides.filter((ride) => ride.id !== confirmedRide.id)
        );
      });
    });

    return () => {
      hubConnection.off('NewRide');
      hubConnection.off('RideConfirmed');
    };
  }, []);

  const handleAcceptRide = async (rideId: string) => {
    try {
      await confirmRide(rideId);
      const updatedPendingRides = await getPendingRides();
      setPendingRides(updatedPendingRides);
    } catch (error) {
      console.error('Failed to accept ride:', error);
    }
  };

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
        <button onClick={handleUpdateProfile} className="update-profile-button">Update Profile</button>
        <button onClick={handleLogout} className="logout-button">Logout</button>
      </div>
      <div className="dashboard-box">
        <h2 className="dashboard-title">Pending Rides</h2>
        <ul className="rides-list">
          {pendingRides.map((ride: Ride) => (
            <li key={ride.id} className="ride-item">
              <div className="ride-item-header">
                <span className="ride-address">{ride.startAddress} to {ride.endAddress}</span>
                <button onClick={() => handleAcceptRide(ride.id)} className="accept-ride-button">Accept Ride</button>
              </div>
              <div className="ride-details">
                <span className="ride-info">Price: ${ride.price}</span>
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

export default DriverDashboardPage;
