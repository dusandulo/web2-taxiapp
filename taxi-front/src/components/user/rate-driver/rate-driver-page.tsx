import React, { useState } from 'react';
import './RateDriverPage.css';

const RateDriver: React.FC = () => {
  const [rating, setRating] = useState<number>(0);

  const handleRate = (value: number) => {
    setRating(value);
    // Logika za slanje ocene na server
  };

  return (
    <div className="rate-driver-container">
      <h2>Rate Your Driver</h2>
      <div className="stars">
        {[1, 2, 3, 4, 5].map((star) => (
          <span
            key={star}
            className={`star ${star <= rating ? 'selected' : ''}`}
            onClick={() => handleRate(star)}
          >
            ★
          </span>
        ))}
      </div>
    </div>
  );
};

export default RateDriver;
