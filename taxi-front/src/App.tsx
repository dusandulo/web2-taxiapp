import React, { useEffect, useState } from 'react';
import { Route, Routes, useNavigate } from 'react-router-dom';
import Login from './components/login/login-page';
import Register from './components/register/register-page';
import UserDashboardPage from './components/user/user-dashboard/user-dashboard-page';
import DriverDashboardPage from './components/driver/driver-dashboard-page';
import AdminDashboardPage from './components/admin/admin-dashboard-page';
import CreateRide from './components/user/create-ride/create-ride-page';
import RideWaiting from './components/user/ride-waiting/ride-waiting-page';
import UpdateProfilePage from './components/user/update-profile/update-profile-page';
import ProtectedRoute from './components/ProtectedRoute';
import RatingPage from './components/user/rate-driver/rate-page'; // Import RatingPage
import { GoogleOAuthProvider } from '@react-oauth/google';

const App: React.FC = () => {
  const navigate = useNavigate();
  const [userRole, setUserRole] = useState<string | null>(localStorage.getItem('role'));
  const clientId = process.env.REACT_APP_GOOGLE_CLIENT_ID!;

  useEffect(() => {
    const role = localStorage.getItem('role');
    if (role !== userRole) {
      setUserRole(role);
      if (role) {
        navigate('/dashboard'); // Navigiraj na odgovarajući dashboard
      } else {
        navigate('/login'); // Ako nema uloge, navigiraj na login
      }
    }
  }, [userRole, navigate]);

  const renderDashboard = () => {
    switch (userRole) {
      case 'User':
        return <UserDashboardPage />;
      case 'Driver':
        return <DriverDashboardPage />;
      case 'Admin':
        return <AdminDashboardPage />;
      default:
        return <Login />;
    }
  };

  return (
    <GoogleOAuthProvider clientId={clientId}>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        
        <Route element={<ProtectedRoute />}>
          <Route path="/dashboard" element={renderDashboard()} />
          <Route path="/create-ride" element={<CreateRide />} />
          <Route path="/ride-waiting/:rideId" element={<RideWaiting />} />
          <Route path="/update-profile" element={<UpdateProfilePage />} />
          <Route path="/rate/:rideId/:driverId" element={<RatingPage />} /> {/* Add the route for RatingPage */}
        </Route>
      </Routes>
    </GoogleOAuthProvider>
  );
}

export default App;
