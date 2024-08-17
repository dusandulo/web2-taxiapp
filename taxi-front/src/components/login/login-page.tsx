import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { login } from '../../services/authService';
import { register } from '../../services/registerService';
import './LoginPage.css';
import logo from '../../images/logo.png';
import { Link } from 'react-router-dom';
import { GoogleLogin, CredentialResponse } from '@react-oauth/google';
import { jwtDecode } from 'jwt-decode';

const Login: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();

  interface TokenPayload {
    given_name: string;
    family_name: string;
    email: string;
  }

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const response = await login(email, password);

      localStorage.setItem('token', response.data.token);
      localStorage.setItem('userId', response.data.userId);
      localStorage.setItem('role', response.data.role);

      navigate('/dashboard');
    } catch (error) {
      console.error('Login failed:', error);
      alert('Login failed, please check your credentials');
    }
  };

  const googleLogIn = (response: { token: string, userId: string, role: string }) => {
    localStorage.setItem('token', response.token);
    localStorage.setItem('userId', response.userId);
    localStorage.setItem('role', response.role);
  
    navigate('/dashboard');
  };

  const responseMessage = async (response: CredentialResponse) => {
    const googlePassword = process.env.REACT_APP_GOOGLE_PASSWORD || 'default_password';

    let decodedToken: TokenPayload | undefined;
    try {
        decodedToken = jwtDecode<TokenPayload>(response.credential as string);
        const loginResponse = await login(decodedToken.email, googlePassword);

        if (loginResponse.data.token) {
            googleLogIn({
                token: loginResponse.data.token,
                userId: loginResponse.data.userId,
                role: loginResponse.data.role,
            });
        }
    } catch (error) {
        console.error('Error during Google login:', error);
        if (!decodedToken) {
            console.error('Google token decoding failed');
            return;
        }

        try {
            const formData = new FormData();
            formData.append('userName', decodedToken.email);
            formData.append('email', decodedToken.email);
            formData.append('password', googlePassword);
            formData.append('name', decodedToken.given_name || '');
            formData.append('lastName', decodedToken.family_name || '');
            formData.append('birthday', '2024-08-01');
            formData.append('address', 'Change address');
            formData.append('role', '1');
            formData.append('image', null as any);

            await register(formData);

            const loginResponse = await login(decodedToken.email, googlePassword);
            googleLogIn({
                token: loginResponse.data.token,
                userId: loginResponse.data.userId,
                role: loginResponse.data.role,
            });
        } catch (err) {
          console.error('Registration failed:', err);
      }
    }
};




  return (
    <div className="login-container">
      <div className="login-box">
        <h2 className="login-title">Log In</h2>
        <img className="logo-image" src={logo} alt="Taxi Logo" />
        <form className="login-form" onSubmit={handleLogin}>
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
          <button type="submit" className="login-button">
            Log In
          </button>
        </form>
        <p>
          Don't have an account? <Link to="/register">Register here</Link>
        </p>
        <GoogleLogin onSuccess={responseMessage} onError={() => console.log('Login Failed')} />
      </div>
    </div>
  );
};

export default Login;
