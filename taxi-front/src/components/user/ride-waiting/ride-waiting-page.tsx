import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { getRideById } from '../../../services/rideService';
import './RideWaitingPage.css';

const RideWaiting: React.FC = () => {
  const [waitingTime, setWaitingTime] = useState<number | null>(null);
  const [rideTime, setRideTime] = useState<number | null>(null);
  const [status, setStatus] = useState<string | null>(null); // Dodato za praćenje statusa vožnje
  const { rideId } = useParams<{ rideId: string }>(); // Dohvatanje rideId iz URL-a

  useEffect(() => {
    if (!rideId) return;

    // Inicijalno dohvatite podatke o vožnji sa servera
    const fetchRideData = async () => {
      try {
        const rideData = await getRideById(rideId);

        // Postavite početno vreme čekanja i vreme trajanja vožnje
        setWaitingTime(rideData.arrivalTimeInSeconds);
        setRideTime(rideData.driverTimeInSeconds);
        setStatus(rideData.status); // Postavite status vožnje
      } catch (error) {
        console.error('Failed to fetch ride data:', error);
      }
    };

    fetchRideData();

    // Interval za ažuriranje vremena čekanja
    const interval = setInterval(() => {
      setWaitingTime((prevTime) => {
        if (prevTime !== null && prevTime > 0) {
          return prevTime - 1;
        } else if (prevTime === 0) {
          clearInterval(interval);
          startRideTimer(); // Pokrenite odbrojavanje za vožnju
        }
        return prevTime;
      });
    }, 1000);

    return () => clearInterval(interval);
  }, [rideId]);

  const startRideTimer = () => {
    const rideInterval = setInterval(() => {
      setRideTime((prevTime) => {
        if (prevTime !== null && prevTime > 0) {
          return prevTime - 1;
        } else {
          clearInterval(rideInterval);
          // Završetak vožnje
          return 0;
        }
      });
    }, 1000);
  };

  return (
    <div className="ride-waiting-container">
      {status === 'Pending' ? (
        <h2>Waiting for a free driver to accept the ride...</h2>
      ) : waitingTime !== null && waitingTime > 0 ? (
        <div className="countdown-box">
          <h2>Driver will arrive in:</h2>
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
