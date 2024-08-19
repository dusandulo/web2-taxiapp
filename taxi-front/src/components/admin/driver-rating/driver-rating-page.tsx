import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom'; // Importuj useNavigate za navigaciju
import { getDriverRatings } from '../../../services/ratingService';
import { blockDriver, unblockDriver } from '../../../services/userService';
import './DriverRatingPage.css';

interface DriverRating {
  driverId: string;
  driverName: string;
  averageRating: number;
  email: string;
  isBlocked: boolean;
}

const DriverRatingsPage: React.FC = () => {
  const [driverRatings, setDriverRatings] = useState<DriverRating[]>([]);
  const navigate = useNavigate(); // useNavigate hook za navigaciju

  useEffect(() => {
    const fetchDriverRatings = async () => {
      try {
        const ratingsData = await getDriverRatings();
        console.log(ratingsData);
        setDriverRatings(ratingsData);
      } catch (error) {
        console.error('Failed to fetch driver ratings:', error);
      }
    };

    fetchDriverRatings();
  }, []);

  const handleBlockDriver = async (driverId: string) => {
    try {
      await blockDriver(driverId);
      setDriverRatings(prevRatings =>
        prevRatings.map(driver =>
          driver.driverId === driverId ? { ...driver, isBlocked: true } : driver
        )
      );
      alert('Driver blocked successfully.');
    } catch (error) {
      console.error('Failed to block driver:', error);
      alert('Failed to block driver.');
    }
  };

  const handleUnblockDriver = async (driverId: string) => {
    try {
      await unblockDriver(driverId);
      setDriverRatings(prevRatings =>
        prevRatings.map(driver =>
          driver.driverId === driverId ? { ...driver, isBlocked: false } : driver
        )
      );
      alert('Driver unblocked successfully.');
    } catch (error) {
      console.error('Failed to unblock driver:', error);
      alert('Failed to unblock driver.');
    }
  };

  const handleBackToDashboard = () => {
    navigate('/dashboard'); // Navigacija nazad na dashboard
  };

  return (
    <div className="ratings-container">
      <div className="header">
        <h2>Driver Ratings</h2>
        <button
          onClick={handleBackToDashboard}
          className="back-to-dashboard-button"
        >
          Back to Dashboard
        </button>
      </div>
      <ul className="ratings-list">
        {driverRatings.map((driver: DriverRating) => (
          <li key={driver.driverId} className="driver-rating-item">
            <div className="driver-infoo">
              <p>Name: {driver.driverName}</p>
              <p>Email: {driver.email}</p>
              <p>Average Rating: {driver.averageRating.toFixed(2)} ⭐</p>
              {driver.isBlocked ? (
              <button
                onClick={() => handleUnblockDriver(driver.driverId)}
                className="block-button"
              >
                Unblock
              </button>
            ) : (
              <button
                onClick={() => handleBlockDriver(driver.driverId)}
                className="block-button"
              >
                Block
              </button>
            )}
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default DriverRatingsPage;
