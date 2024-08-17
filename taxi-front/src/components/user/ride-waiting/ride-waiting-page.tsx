import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
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
  const { rideId } = useParams<{ rideId: string }>();

  useEffect(() => {
    if (!rideId) return;

    const fetchRideData = async () => {
      try {
        const rideData = await getRideById(rideId);
        console.log('Initial Ride Data:', rideData);
        setWaitingTime(rideData.arrivalTimeInSeconds);
        setRideTime(rideData.arrivalTimeInSeconds);
        setStatus(rideData.status);
      } catch (error) {
        console.error('Failed to fetch ride data:', error);
      }
    };

    fetchRideData();

    startConnection().then(async () => {
      console.log('SignalR Connection Established');
      await hubConnection.invoke('JoinGroup', rideId);
      console.log(`Joined group for ride ${rideId}`);

      subscribeToRideConfirmation((confirmedRide) => {
        console.log('Ride confirmed event received:', confirmedRide);
        if (confirmedRide.id === rideId) {
          setStatus(confirmedRide.status);
          setWaitingTime(confirmedRide.arrivalTimeInSeconds);
          setRideTime(confirmedRide.arrivalTimeInSeconds);
        }
      });

      subscribeToRideTimeUpdate((timeUpdate) => {
        console.log('Ride time update event received on client:', timeUpdate);
        setWaitingTime(timeUpdate);
      });
    });

    return () => {
      hubConnection.off('RideConfirmed');
      hubConnection.off('UpdateRideTime');
    };
  }, [rideId]);

  useEffect(() => {
    if (waitingTime === null) return;

    let myInterval = setInterval(() => {
      if (waitingTime > 0) {
        setWaitingTime(waitingTime - 1);
      } else {
        clearInterval(myInterval);
      }
    }, 1000);

    return () => clearInterval(myInterval);
  }, [waitingTime]);

  return (
    <div className="ride-waiting-container">
      {status === RideStatus.Pending ? (
        <h2>Waiting for a free driver to accept the ride...</h2>
      ) : waitingTime !== null && waitingTime > 0 ? (
        <div className="countdown-box">
          <h2>Your ride will arrive in:</h2>
          <p className="countdown">{waitingTime} seconds</p>
        </div>
      ) : rideTime !== null ? (
        <div className="countdown-box">
          <h2>Ride time left:</h2>
          <p className="countdown">{rideTime} seconds</p>
        </div>
      ) : (
        <h2>Waiting for ride...</h2>
      )}
    </div>
  );
};

export default RideWaiting;
