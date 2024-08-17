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
  const [profileImage, setProfileImage] = useState<File | null>(null); // State for profile image
  const navigate = useNavigate();

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();

    if (password !== confirmPassword) {
      alert("Passwords don't match");
      return;
    }

    // Create a FormData object to handle file uploads
    const formData = new FormData();
    formData.append('userName', userName);
    formData.append('email', email);
    formData.append('password', password);
    formData.append('name', firstName);
    formData.append('lastName', lastName);
    formData.append('address', address);
    formData.append('birthday', birthday);
    formData.append('role', isDriver ? '2' : '1');

    if (profileImage) {
      formData.append('image', profileImage); // Add the profile image file to the form data
    }

    try {
      await register(formData);
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
        <form className="login-form" onSubmit={handleRegister} encType="multipart/form-data">
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
            Profile Image
            <input
              type="file"
              accept="image/*"
              className="login-input"
              onChange={(e) => setProfileImage(e.target.files?.[0] || null)} // Set the profile image file
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
