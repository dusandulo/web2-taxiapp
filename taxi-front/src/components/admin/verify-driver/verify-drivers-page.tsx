import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom'; // Importujte useNavigate za navigaciju
import { getUnverifiedDrivers, verifyDriver } from '../../../services/userService';
import './VerifyDriversPage.css';

interface Driver {
  id: string;
  name: string;
  email: string;
}

const VerifyDriversPage: React.FC = () => {
  const [drivers, setDrivers] = useState<Driver[]>([]);
  const navigate = useNavigate(); // Inicijalizacija useNavigate za navigaciju

  useEffect(() => {
    const fetchUnverifiedDrivers = async () => {
      try {
        const driversData = await getUnverifiedDrivers();
        setDrivers(driversData);
      } catch (error) {
        console.error('Failed to fetch unverified drivers:', error);
      }
    };

    fetchUnverifiedDrivers();
  }, []);

  const handleVerifyDriver = async (driverId: string) => {
    try {
      await verifyDriver(driverId);
      setDrivers((prevDrivers) =>
        prevDrivers.filter((driver) => driver.id !== driverId)
      );
      alert('Driver verified successfully.');
    } catch (error) {
      console.error('Failed to verify driver:', error);
      alert('Failed to verify driver.');
    }
  };

  const handleBackToDashboard = () => {
    navigate('/dashboard'); // Navigacija nazad na dashboard
  };

  return (
    <div className="verify-drivers-container">
      <div className="header">
        <h2>Unverified Drivers</h2>
        <button
          onClick={handleBackToDashboard}
          className="back-to-dashboard-button"
        >
          Back to Dashboard
        </button>
      </div>
      <ul className="drivers-list">
        {drivers.map((driver) => (
          <li key={driver.id} className="driver-item">
            <div className="driver-info">
              <p>Name: {driver.name}</p>
              <p>Email: {driver.email}</p>
            </div>
            <button
              onClick={() => handleVerifyDriver(driver.id)}
              className="verify-button"
            >
              Verify
            </button>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default VerifyDriversPage;
