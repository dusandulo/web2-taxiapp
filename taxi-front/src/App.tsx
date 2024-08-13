import React from 'react';
import { Route, Routes } from 'react-router-dom';
import Login from './components/login/login-page';
import Register from './components/register/register-page';
import Dashboard from './components/dashboard/dashboard-page';
import CreateRide from './components/user/create-ride/create-ride-page';
import RideWaiting from './components/user/ride-waiting/ride-waiting-page';
import UpdateProfilePage from './components/user/update-profile/update-profile-page';


const App: React.FC = () => {
  return (
    <Routes>
      <Route path="/" element={<Login />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="/dashboard" element={<Dashboard />} />
      <Route path="/create-ride" element={<CreateRide />} />
      <Route path="/ride-waiting/:rideId" element={<RideWaiting />} />
      <Route path="/update-profile" element={<UpdateProfilePage />} />
    </Routes>
  );
}

export default App;