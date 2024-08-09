import React, { useEffect, useState } from 'react';
import axios from 'axios';

const Dashboard: React.FC = () => {
  const [rides, setRides] = useState([]);

  useEffect(() => {
    const fetchRides = async () => {
      const token = localStorage.getItem('token');
      const response = await axios.get('/api/rides', {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });
      setRides(response.data);
    };

    fetchRides();
  }, []);

  return (
    <div>
      <h2>Your Rides</h2>
      <ul>
        {rides.map((ride: any) => (
          <li key={ride.id}>{ride.startAddress} to {ride.endAddress}</li>
        ))}
      </ul>
    </div>
  );
};

export default Dashboard;