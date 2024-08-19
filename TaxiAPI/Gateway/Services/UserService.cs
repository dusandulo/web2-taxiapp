using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net.Mail;
using System.Net;

namespace Communication
{
    public class UserService : IUserCommunication
    {
        private readonly IUserCommunication _userCommunication;
        private readonly string _key;
        private readonly string _imageFolderPath;

        public UserService(IConfiguration configuration)
        {
            _userCommunication = ServiceProxy.Create<IUserCommunication>(
                new Uri("fabric:/TaxiAPI/UserStateful"), new ServicePartitionKey(1));
            _key = configuration["Jwt:Key"];
            _imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        }

        public async Task Register(UserModel user)
        {
            ValidateUserModel(user);

            var existingUser = await _userCommunication.GetUserByEmail(user.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists.");
            }

            user.Id = Guid.NewGuid();
            user.Password = HashPassword(user.Password);
            user.VerificationState = user.Role == UserRole.Driver ? VerificationState.Unverified : VerificationState.Verified;

            await _userCommunication.Register(user);
        }

        public async Task<string?> Authenticate(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Email and password are required.");
            }

            var user = await _userCommunication.GetUserByEmail(email);
            if (user == null || !VerifyPassword(password, user.Password))
            {
                return null;
            }

            return GenerateJwtToken(user);
        }

        public async Task<UserModel?> GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is required.");
            }

            return await _userCommunication.GetUserByEmail(email);
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            return await _userCommunication.GetAllUsers();
        }

        public async Task UpdateProfile(UserModel user)
        {
            if (user == null || user.Id == Guid.Empty)
            {
                throw new ArgumentException("User and valid user ID are required.");
            }

            ValidateUserModel(user, isUpdate: true);

            await _userCommunication.UpdateProfile(user);
        }
        private string GenerateJwtToken(UserModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "your_issuer",
                Audience = "your_audience",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password ?? "");
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        private void ValidateUserModel(UserModel user, bool isUpdate = false)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("Email is required.", nameof(user.Email));

            if (string.IsNullOrWhiteSpace(user.UserName))
                throw new ArgumentException("UserName is required.", nameof(user.UserName));

            if (!isUpdate && string.IsNullOrWhiteSpace(user.Password))
                throw new ArgumentException("Password is required.", nameof(user.Password));

            if (string.IsNullOrWhiteSpace(user.Name))
                throw new ArgumentException("Name is required.", nameof(user.Name));

            if (string.IsNullOrWhiteSpace(user.LastName))
                throw new ArgumentException("LastName is required.", nameof(user.LastName));
        }

        public async Task<bool> UpdateVerificationState(Guid userId, VerificationState state)
        {
            var success = await _userCommunication.UpdateVerificationState(userId, state);
            if (success && state == VerificationState.Verified)
            {
                var user = await GetUserById(userId);
                if (user != null)
                {
                    SendVerificationEmail(user.Email, user.Name);
                }
            }
            return success;
        }

        private void SendVerificationEmail(string toEmail, string userName)
        {
            var smtpClient = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("c94feac755e586", "46f367e560adf7"),
                EnableSsl = true
            };

            var fromEmail = "administrator@taxiapi.com";
            var subject = "[Taxi API] Your profile has been verified";
            var body = $"Dear {userName},\n\nYour profile has been successfully verified.\nBest regards,\nTaxi API";

            var mailMessage = new MailMessage(fromEmail, toEmail, subject, body);

            smtpClient.Send(mailMessage);
        }

        public async Task<IEnumerable<UserModel>> GetUnverifiedDrivers()
        {
            return await _userCommunication.GetUnverifiedDrivers();
        }

        public async Task<UserModel?> GetUserById(Guid userId)
        {
            return await _userCommunication.GetUserById(userId);
        }
    }
}
