import React, { useState, useEffect } from 'react';
import { getUserProfile, updateUserProfile } from '../../../services/userService';
import { Link } from 'react-router-dom';
import './UpdateProfilePage.css';

const UpdateProfilePage: React.FC = () => {
  const [user, setUser] = useState({
    id: '',
    userName: '',
    email: '',
    password: '',
    name: '',
    lastName: '',
    address: '',
    birthday: '',
    role: '',
    image: '',  // Dodato polje za putanju slike
  });

  const [imageUrl, setImageUrl] = useState<string | null>(null); // Drži puni URL slike

  useEffect(() => {
    const fetchUserProfile = async () => {
      try {
        const userProfile = await getUserProfile();
        setUser({ ...userProfile, password: '' });

        // Ako postoji putanja slike, generiši puni URL
        if (userProfile.image) {
          setImageUrl(`${process.env.REACT_APP_IMAGE_BASE_URL}/${userProfile.image}`);
        }
      } catch (error) {
        console.error('Failed to fetch user profile:', error);
      }
    };

    fetchUserProfile();
  }, []);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setUser({
      ...user,
      [name]: value,
    });
  };

  const handleUpdateProfile = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      await updateUserProfile(user);
      alert('Profile updated successfully!');
    } catch (error) {
      console.error('Failed to update profile:', error);
      alert('Failed to update profile, please try again.');
    }
  };

  return (
    <div className="update-profile-container">
        <Link to="/dashboard" className="back-to-dashboard-link">Back to dashboard</Link>
      <div className="update-profile-box">
        <h2 className="update-profile-title">Update Profile</h2>
        {imageUrl && (
          <div className="profile-image-container">
            <img src={imageUrl} alt="Profile" className="profile-image" />
          </div>
        )}
        <form className="update-profile-form" onSubmit={handleUpdateProfile}>
          <label className="update-profile-label">
            Username
            <input
              type="text"
              name="userName"
              className="update-profile-input"
              value={user.userName}
              onChange={handleInputChange}
              required
            />
          </label>
          <label className="update-profile-label">
            Email
            <input
              type="email"
              name="email"
              className="update-profile-input"
              value={user.email}
              onChange={handleInputChange}
              required
            />
          </label>
          <label className="update-profile-label">
            Password (must enter to finish update)
            <input
              type="password"
              name="password"
              className="update-profile-input"
              value={user.password}
              onChange={handleInputChange}
            />
          </label>
          <label className="update-profile-label">
            First Name
            <input
              type="text"
              name="name"
              className="update-profile-input"
              value={user.name}
              onChange={handleInputChange}
              required
            />
          </label>
          <label className="update-profile-label">
            Last Name
            <input
              type="text"
              name="lastName"
              className="update-profile-input"
              value={user.lastName}
              onChange={handleInputChange}
              required
            />
          </label>
          <label className="update-profile-label">
            Address
            <input
              type="text"
              name="address"
              className="update-profile-input"
              value={user.address}
              onChange={handleInputChange}
              required
            />
          </label>
          <label className="update-profile-label">
            Birthday
            <input
              type="date"
              name="birthday"
              className="update-profile-input"
              value={user.birthday}
              onChange={handleInputChange}
              required
            />
          </label>
          <button type="submit" className="update-profile-button">Update Profile</button>
        </form>
      </div>
    </div>
  );
};

export default UpdateProfilePage;
