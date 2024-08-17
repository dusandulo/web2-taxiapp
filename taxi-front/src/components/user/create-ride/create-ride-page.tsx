import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { estimateRide, createRide } from '../../../services/rideService';
import './CreateRidePage.css';

const CreateRide: React.FC = () => {
  const [startAddress, setStartAddress] = useState('');
  const [endAddress, setEndAddress] = useState('');
  const [estimatedPrice, setEstimatedPrice] = useState<number | null>(null);
  const [estimatedTime, setEstimatedTime] = useState<number | null>(null);
  const navigate = useNavigate();

  const handleEstimate = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      const { price, time } = await estimateRide(startAddress, endAddress);
      setEstimatedPrice(price);
      setEstimatedTime(time);
    } catch (error) {
      console.error('Failed to estimate ride:', error);
      alert('Failed to estimate ride, please try again.');
    }
  };

  const handleConfirm = async () => {
    if (estimatedPrice === null || estimatedTime === null) return;

    try {
      // Kreiranje vožnje
      const rideData = {
        startAddress: startAddress,
        endAddress: endAddress,
        price: estimatedPrice,
        arrivalTimeInSeconds: estimatedTime
      };

      const createdRide = await createRide(rideData);

      // Preusmeravanje na stranicu za čekanje vožnje sa ID-jem vožnje
      navigate(`/ride-waiting/${createdRide.id}`);
    } catch (error) {
      console.error('Failed to create ride:', error);
      alert('Failed to create ride, please try again.');
    }
  };

  return (
    <div className="create-ride-container">
      <div className="create-ride-box">
        <h2 className="create-ride-title">Create New Ride</h2>
        <form className="create-ride-form" onSubmit={handleEstimate}>
          <label className="create-ride-label">
            Start Address
            <input
              type="text"
              placeholder="Enter start address"
              className="create-ride-input"
              value={startAddress}
              onChange={(e) => setStartAddress(e.target.value)}
              required
            />
          </label>
          <label className="create-ride-label">
            End Address
            <input
              type="text"
              placeholder="Enter end address"
              className="create-ride-input"
              value={endAddress}
              onChange={(e) => setEndAddress(e.target.value)}
              required
            />
          </label>
          <button type="submit" className="create-ride-button">Estimate time</button>
        </form>

        {estimatedPrice !== null && estimatedTime !== null && (
          <div className="ride-estimation">
            <p>Estimated Price: {estimatedPrice} RSD</p>
            <p>Estimated Waiting Time: {estimatedTime} seconds</p>
            <button onClick={handleConfirm} className="create-ride-button">Order drive</button>
          </div>
        )}
      </div>
    </div>
  );
};

export default CreateRide;
