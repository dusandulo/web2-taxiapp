import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getRideById } from '../../../services/rideService';
import hubConnection, { startConnection, subscribeToRideConfirmation, subscribeToRideTimeUpdate } from '../../../services/signalRService';
import './RideWaitingPage.css';

enum RideStatus {
  Pending = 0,
  Confirmed = 1,
  Finished = 2,
}

const RideWaiting: React.FC = () => {
  const [waitingTime, setWaitingTime] = useState<number | null>(null);
  const [rideTime, setRideTime] = useState<number | null>(null);
  const [status, setStatus] = useState<RideStatus | null>(null);
  const [driverId, setDriverId] = useState<string | null>(null);
  const { rideId } = useParams<{ rideId: string }>();
  const navigate = useNavigate();

  useEffect(() => {
    if (!rideId) return;

    const fetchRideData = async () => {
      try {
        const rideData = await getRideById(rideId);
        console.log('Initial Ride Data:', rideData);
        setWaitingTime(rideData.arrivalTimeInSeconds);
        setRideTime(rideData.driverTimeInSeconds);
        setStatus(rideData.status);
        setDriverId(rideData.driverId || null);
      } catch (error) {
        console.error('Failed to fetch ride data:', error);
      }
    };

    fetchRideData();

    const connectToHub = async () => {
      try {
        await startConnection(); // Osigurajte da je SignalR veza uspostavljena
        console.log('SignalR Connection Established');
        await hubConnection.invoke('JoinGroup', rideId);
        console.log(`Joined group for ride ${rideId}`);

        subscribeToRideConfirmation((confirmedRide) => {
          console.log('Ride confirmed event received:', confirmedRide);
          if (confirmedRide.id === rideId) {
            setStatus(confirmedRide.status);
            setWaitingTime(confirmedRide.arrivalTimeInSeconds);
            setRideTime(confirmedRide.driverTimeInSeconds);
            setDriverId(confirmedRide.driverId);
          }
        });

        subscribeToRideTimeUpdate((timeUpdate) => {
          console.log('Ride time update event received on client:', timeUpdate);
          setWaitingTime(timeUpdate);
        });

        hubConnection.on('RideFinished', (finishedRide) => {
          console.log('Ride finished event received');
          navigate(`/rate/${rideId}/${driverId}`);
        });

      } catch (error) {
        console.error('Error connecting to SignalR:', error);
      }
    };

    connectToHub();

    return () => {
      hubConnection.off('RideConfirmed');
      hubConnection.off('UpdateRideTime');
      hubConnection.off('RideFinished');
    };
  }, [rideId, navigate, driverId]);

  useEffect(() => {
    if (waitingTime === null) return;

    const myInterval = setInterval(() => {
      if (waitingTime > 0) {
        setWaitingTime(waitingTime - 1);
      } else {
        clearInterval(myInterval);
        setRideTime(0);
      }
    }, 1000);

    return () => clearInterval(myInterval);
  }, [waitingTime]);

  useEffect(() => {
    if (rideTime === null || rideTime < 0) return;

    const rideInterval = setInterval(() => {
      setRideTime(prev => (prev !== null ? prev + 1 : 0));
    }, 1000);

    return () => clearInterval(rideInterval);
  }, [rideTime]);

  return (
    <div className="ride-waiting-container">
      {status === RideStatus.Pending ? (
        <h2>Waiting for a free driver to accept the ride...</h2>
      ) : status === RideStatus.Finished ? (
        <h2>Ride finished</h2>
      ) : waitingTime !== null && waitingTime > 0 ? (
        <div className="countdown-box">
          <h2>Your ride will arrive in:</h2>
          <p className="countdown">{waitingTime} seconds</p>
        </div>
      ) : rideTime !== null ? (
        <div className="countdown-box">
          <h2>The ride is in progress:</h2>
          <p className="countdown">{rideTime} seconds</p>
        </div>
      ) : (
        <h2>Waiting for ride...</h2>
      )}
    </div>
  );
};

export default RideWaiting;
