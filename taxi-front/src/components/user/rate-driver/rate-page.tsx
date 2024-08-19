import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { addRating } from '../../../services/ratingService';
import './RatingPage.css';

const RatingPage: React.FC = () => {
  const { rideId, driverId } = useParams<{ rideId: string; driverId: string }>();
  const [ratingValue, setRatingValue] = useState<number>(0);
  const [comment, setComment] = useState<string>('');
  const navigate = useNavigate();

  const handleSubmit = async () => {
    if (!rideId || !driverId || driverId === 'undefined' || ratingValue < 1 || ratingValue > 5) {
      alert('Please provide a valid rating and comment.');
      return;
    }
  
    console.log("Submitting rating with values:", {
      rideId,
      driverId,
      passengerId: localStorage.getItem('userId'),
      ratingValue,
      comment,
    });
  
    try {
      await addRating({
        rideId,
        driverId,
        passengerId: localStorage.getItem('userId') || '',
        ratingValue,
        comment,
      });
      alert('Rating submitted successfully!');
      navigate('/dashboard');
    } catch (error) {
      console.error('Failed to submit rating:', error);
      alert('Failed to submit rating.');
    }
  };

  return (
    <div className="rating-container">
      <h2>Rate Your Driver</h2>
      <div className="rating-stars">
        {[1, 2, 3, 4, 5].map((value) => (
          <span
            key={value}
            className={value <= ratingValue ? 'star filled' : 'star'}
            onClick={() => setRatingValue(value)}
          >
            ★
          </span>
        ))}
      </div>
      <textarea
        placeholder="Leave a comment..."
        value={comment}
        onChange={(e) => setComment(e.target.value)}
      />
      <button onClick={handleSubmit} className="submit-rating-button">
        Submit Rating
      </button>
    </div>
  );
};

export default RatingPage;
