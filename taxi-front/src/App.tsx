import React from 'react';
import { Route, Routes } from 'react-router-dom';
import Login from './components/login/login-page';
import Register from './components/register/register-page';
import Dashboard from './components/dashboard/dashboard-page';

const App: React.FC = () => {
  return (
    <Routes>
      <Route path="/" element={<Login />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="/dashboard" element={<Dashboard />} />
    </Routes>
  );
}

export default App;