import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { register } from '../../services/registerService';
import './RegisterPage.css';
import logo from '../../images/logo.png';
import { Link } from 'react-router-dom';

const Register: React.FC = () => {
  const [userName, setUserName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [address, setAddress] = useState('');
  const [birthday, setBirthday] = useState('');
  const [isDriver, setIsDriver] = useState(false); 
  const navigate = useNavigate();

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();

    if (password !== confirmPassword) {
      alert("Passwords don't match");
      return;
    }

    try {
      await register({
        userName,
        email,
        password,
        name: firstName,
        lastName,
        address,
        birthday,
        role: isDriver ? 2 : 1
      });

      navigate('/login');
    } catch (error) {
      console.error('Registration failed:', error);
      alert('Registration failed, please check your input and try again');
    }
  };

  return (
    <div className="login-container">
      <div className="login-box">
        <h2 className="login-title">Register</h2>
        <img className='logo-image' src={logo} alt="Taxi Logo"/>
        <form className="login-form" onSubmit={handleRegister}>
          <label className="login-label">
            Username
            <input
              type="text"
              placeholder="Enter your username"
              className="login-input"
              value={userName}
              onChange={(e) => setUserName(e.target.value)}
              required
            />
          </label>
          <label className="login-label">
            Email
            <input
              type="email"
              placeholder="Enter your email"
              className="login-input"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </label>
          <label className="login-label">
            Password
            <input
              type="password"
              placeholder="Enter your password"
              className="login-input"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </label>
          <label className="login-label">
            Confirm Password
            <input
              type="password"
              placeholder="Confirm your password"
              className="login-input"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
            />
          </label>
          <label className="login-label">
            First Name
            <input
              type="text"
              placeholder="Enter your first name"
              className="login-input"
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
              required
            />
          </label>
          <label className="login-label">
            Last Name
            <input
              type="text"
              placeholder="Enter your last name"
              className="login-input"
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
              required
            />
          </label>
          <label className="login-label">
            Address
            <input
              type="text"
              placeholder="Enter your address"
              className="login-input"
              value={address}
              onChange={(e) => setAddress(e.target.value)}
              required
            />
          </label>
          <label className="login-label">
            Birthday
            <input
              type="date"
              className="login-input"
              value={birthday}
              onChange={(e) => setBirthday(e.target.value)}
              required
            />
          </label>
          <label className="login-label">
            Are you a driver?
            <input
              type="checkbox"
              checked={isDriver}
              onChange={(e) => setIsDriver(e.target.checked)}
              className="login-checkbox"
            />
          </label>
          <button type="submit" className="login-button">Register</button>
        </form>
        <p>Already have an account? <Link to="/login">Log in here</Link></p>
      </div>
    </div>
  );
};

export default Register;
