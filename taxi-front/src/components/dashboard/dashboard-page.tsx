import React, { useEffect, useState } from 'react';
import { getAllRides, getPendingRides, confirmRide } from '../../services/rideService';
import { Link, useNavigate } from 'react-router-dom';
import hubConnection, { startConnection, subscribeToNewRides, subscribeToRideConfirmation } from '../../services/signalRService';
import './DashboardPage.css';

interface Ride {
  id: string;
  startAddress: string;
  endAddress: string;
  price: number;
  driverTimeInSeconds: number;
  arrivalTimeInSeconds: number;
  status: number;
}

const Dashboard: React.FC = () => {
  const [rides, setRides] = useState<Ride[]>([]);
  const [pendingRides, setPendingRides] = useState<Ride[]>([]);
  const userId = localStorage.getItem('userId');
  const userRole = localStorage.getItem('role');
  const navigate = useNavigate();

  useEffect(() => {
    const fetchRides = async () => {
      try {
        console.log('User ID:', userId);
        console.log('User Role:', userRole);
  
        if (userRole === 'User') {
          const ridesData = await getAllRides(userId!);
          console.log('Fetched rides for user:', ridesData);
          setRides(ridesData);
        }
  
        const pendingRidesData = await getPendingRides();
        console.log('Fetched pending rides:', pendingRidesData);
        setPendingRides(pendingRidesData);
      } catch (error) {
        console.error('Failed to fetch rides:', error);
      }
    };
  
    fetchRides();
  
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
        setRides((prevRides) => [...prevRides, newRide]);
      });
  
      subscribeToRideConfirmation((confirmedRide: Ride) => {
        setPendingRides((prevPendingRides) =>
          prevPendingRides.filter((ride) => ride.id !== confirmedRide.id)
        );
        setRides((prevRides) => [...prevRides, confirmedRide]);
      });
    });
  
    return () => {
      hubConnection.off('NewRide');
      hubConnection.off('RideConfirmed');
    };
  }, [userId, userRole]);
  

  const handleAcceptRide = async (rideId: string) => {
    try {
      await confirmRide(rideId);
      const updatedRides = await getAllRides(userId!);
      setRides(updatedRides);

      const pendingRidesData = await getPendingRides();
      setPendingRides(pendingRidesData);
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
        {userRole === 'User' && (
          <Link to="/create-ride" className="create-ride-link">Create Ride</Link>
        )}
        <Link to="/update-profile" className="update-profile-link">Update Profile</Link>
        <button onClick={handleLogout} className="logout-button">Logout</button>
      </div>
      <div className="dashboard-box">
        {userRole === 'User' && (
          <>
            <h2 className="dashboard-title">Your Rides</h2>
            <ul className="rides-list">
              {rides.map((ride: any) => (
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
          </>
        )}

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
