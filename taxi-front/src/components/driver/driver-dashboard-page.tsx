import React, { useState, useEffect } from 'react';
import { getAllRides, confirmRide, finishRide } from '../../services/rideService';
import { getUserProfileById } from '../../services/userService';
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

enum VerificationState {
  Verified = 'Verified',
  Unverified = 'Unverified',
  Blocked = 'Blocked',
}

const DriverDashboardPage: React.FC = () => {
  const [pendingAndActiveRides, setPendingAndActiveRides] = useState<Ride[]>([]);
  const [finishedRides, setFinishedRides] = useState<Ride[]>([]);
  const [arrivalTimers, setArrivalTimers] = useState<{ [key: string]: number }>({});
  const [rideTimers, setRideTimers] = useState<{ [key: string]: number }>({});
  const [userVerificationState, setUserVerificationState] = useState<VerificationState | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchUserProfile = async () => {
      try {
        const userId = localStorage.getItem('userId');
        if (!userId) {
          navigate('/login');
          return;
        }

        const userProfile = await getUserProfileById(userId); // Preuzmi profil prema ID-u
        setUserVerificationState(userProfile.verificationState);

        if (userProfile.verificationState !== VerificationState.Verified) {
          return; // Ako korisnik nije verifikovan, ne učitavajte ostale podatke
        }

        fetchRides();
      } catch (error) {
        console.error('Failed to fetch user profile:', error);
        navigate('/login');
      }
    };

    const fetchRides = async () => {
      try {
        const userId = localStorage.getItem('userId');
        if (!userId) return;

        const allRidesData = await getAllRides(userId);
        setPendingAndActiveRides(allRidesData.filter((ride: Ride) => ride.status !== 2));
        setFinishedRides(allRidesData.filter((ride: Ride) => ride.status === 2));

        const initialArrivalTimers: { [key: string]: number } = {};
        allRidesData.forEach((ride: Ride) => {
          if (ride.status === 1 && ride.arrivalTimeInSeconds > 0) {
            initialArrivalTimers[ride.id] = ride.arrivalTimeInSeconds;
          }
        });
        setArrivalTimers(initialArrivalTimers);
      } catch (error) {
        console.error('Failed to fetch rides:', error);
      }
    };

    fetchUserProfile();
  }, [navigate]);

  useEffect(() => {
    if (userVerificationState !== VerificationState.Verified) return;

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
        setPendingAndActiveRides((prevRides) => [...prevRides, newRide]);
      });

      subscribeToRideConfirmation((confirmedRide: Ride) => {
        setPendingAndActiveRides((prevRides) =>
          prevRides.map((ride) =>
            ride.id === confirmedRide.id ? { ...ride, status: confirmedRide.status, arrivalTimeInSeconds: confirmedRide.arrivalTimeInSeconds } : ride
          )
        );

        setArrivalTimers((prevTimers) => ({
          ...prevTimers,
          [confirmedRide.id]: confirmedRide.arrivalTimeInSeconds,
        }));
      });
    });

    return () => {
      hubConnection.off('NewRide');
      hubConnection.off('RideConfirmed');
    };
  }, [userVerificationState]);

  useEffect(() => {
    const arrivalInterval = setInterval(() => {
      setArrivalTimers((prevTimers) => {
        const newTimers = { ...prevTimers };
        Object.keys(newTimers).forEach((rideId) => {
          if (newTimers[rideId] > 0) {
            newTimers[rideId] -= 1;
          } else if (newTimers[rideId] === 0 && !(rideId in rideTimers)) {
            setRideTimers((prevRideTimers) => ({
              ...prevRideTimers,
              [rideId]: 0,
            }));
          }
        });
        return newTimers;
      });

      setRideTimers((prevRideTimers) => {
        const newRideTimers = { ...prevRideTimers };
        Object.keys(newRideTimers).forEach((rideId) => {
          if (newRideTimers[rideId] !== undefined) {
            newRideTimers[rideId] += 1;
          }
        });
        return newRideTimers;
      });
    }, 1000);

    return () => clearInterval(arrivalInterval);
  }, [arrivalTimers, rideTimers]);

  const handleAcceptRide = async (rideId: string) => {
    try {
      await confirmRide(rideId);

      setPendingAndActiveRides((prevRides) =>
        prevRides.map((ride) =>
          ride.id === rideId ? { ...ride, status: 1 } : ride
        )
      );

      setArrivalTimers((prevTimers) => ({
        ...prevTimers,
        [rideId]: pendingAndActiveRides.find(ride => ride.id === rideId)?.arrivalTimeInSeconds || 0,
      }));
    } catch (error) {
      console.error('Failed to accept ride:', error);
    }
  };

  const handleFinishRide = async (rideId: string) => {
    if (arrivalTimers[rideId] > 0) return;

    const rideTimeInSeconds = rideTimers[rideId] || 0;

    try {
      await finishRide(rideId, rideTimeInSeconds);

      setPendingAndActiveRides((prevRides) =>
        prevRides.map((ride) =>
          ride.id === rideId ? { ...ride, status: 2 } : ride
        )
      );

      setFinishedRides((prevFinishedRides) =>
        [...prevFinishedRides, pendingAndActiveRides.find((ride) => ride.id === rideId)!]
      );

      setRideTimers((prevRideTimers) => {
        const newTimers = { ...prevRideTimers };
        delete newTimers[rideId];
        return newTimers;
      });

      setArrivalTimers((prevArrivalTimers) => {
        const newArrivalTimers = { ...prevArrivalTimers };
        delete newArrivalTimers[rideId];
        return newArrivalTimers;
      });

    } catch (error) {
      console.error('Failed to finish ride:', error);
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

  if (userVerificationState === VerificationState.Blocked) {
    return (
      <div className="blocked-container">
        <h1>Your account has been blocked</h1>
        <p>
          Unfortunately, your account is currently blocked. Please contact our support team for further assistance or to resolve any issues.
        </p>
      </div>
    );
  }
  
  if (userVerificationState === VerificationState.Unverified) {
    return (
      <div className="unverified-container">
        <h1>Your account is not verified yet</h1>
        <p>
          Your account is pending verification. Our team is reviewing your details. Please check back later or contact support if you have any questions.
        </p>
      </div>
    );
  }

  return (
    <div className="dashboard-container">
      <div className="navigation-buttons">
        <button onClick={handleUpdateProfile} className="update-profile-button">Update Profile</button>
        <button onClick={handleLogout} className="logout-button">Logout</button>
      </div>
      <div className="dashboard-box">
        <h2 className="dashboard-title">Pending and Active Rides</h2>
        <ul className="rides-list">
          {pendingAndActiveRides.map((ride: Ride) => (
            <li key={ride.id} className="ride-item">
              <div className="ride-item-header">
                <span className="ride-address">{ride.startAddress} to {ride.endAddress}</span>
              </div>
              <div className="ride-details">
                <span className="ride-info">Price: {ride.price}RSD</span>
                <span className={`ride-status ${getStatusClass(ride.status)}`}>
                  {ride.status === 0 && 'PENDING'}
                  {ride.status === 1 && 'CONFIRMED'}
                  {ride.status === 2 && 'FINISHED'}
                </span>
                <div className='acceptance-button'>
                  {ride.status === 0 && (
                    <button onClick={() => handleAcceptRide(ride.id)} className="accept-ride-button">Accept Ride</button>
                  )}
                </div>
              </div>
              {ride.status === 1 && (
                <>
                  <div>
                    {rideTimers[ride.id] !== undefined && (
                      <div className="ride-time">Ride Time: {rideTimers[ride.id]} seconds</div>
                    )}
                  </div>
                  <button
                    onClick={() => handleFinishRide(ride.id)}
                    className="finish-ride-button"
                    disabled={arrivalTimers[ride.id] > 0}
                  >
                    Finish Ride {arrivalTimers[ride.id] > 0 && `(${arrivalTimers[ride.id]}s)`}
                  </button>
                </>
              )}
            </li>
          ))}
        </ul>

        <h2 className="dashboard-title">Previous Rides</h2>
        <ul className="rides-list">
          {finishedRides.map((ride: Ride) => (
            <li key={ride.id} className="ride-item">
              <div className="ride-item-header">
                <span className="ride-address">From: {ride.startAddress} To: {ride.endAddress}</span>
              </div>
              <div className="ride-details">
                <span className="ride-info">Price: {ride.price}RSD</span>
                <span className="ride-info">Ride Time: {ride.driverTimeInSeconds} seconds</span>
                <span className={`ride-status ${getStatusClass(ride.status)}`}>
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
