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
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace Communication
{
    public class UserService : IUserCommunication
    {
        private readonly IUserCommunication _userCommunication;
        private readonly string _key;

        public UserService(IConfiguration configuration)
        {
            _userCommunication = ServiceProxy.Create<IUserCommunication>(
                new Uri("fabric:/TaxiAPI/UserStateful"), new ServicePartitionKey(1));
            _key = configuration["Jwt:Key"];
        }

        public async Task Register(UserModel user)
        {
            var existingUser = await _userCommunication.GetUserByEmail(user.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists.");
            }

            var newUser = new UserModel
            {
                Id = Guid.NewGuid(),
                UserName = user.UserName,
                Email = user.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password ?? ""),
                Name = user.Name,
                LastName = user.LastName,
                Birthday = user.Birthday ?? "",
                Address = user.Address ?? "",
                Role = user.Role,
                Image = user.Image ?? "",
                VerificationState = user.Role == UserRole.Driver ? VerificationState.Processing : VerificationState.Verified
            };

            await _userCommunication.Register(newUser);
        }

        public async Task<string?> Authenticate(string email, string password)
        {
            var user = await _userCommunication.GetUserByEmail(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "your_issuer",  // proveri da se ovo poklapa sa vrednostima u appsettings.json
                Audience = "your_audience", // proveri da se ovo poklapa sa vrednostima u appsettings.json
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<UserModel?> GetUserByEmail(string email)
        {
            return await _userCommunication.GetUserByEmail(email);
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            return await _userCommunication.GetAllUsers();
        }

        public async Task UpdateProfile(UserModel user) 
        {
            await _userCommunication.UpdateProfile(user);
        }
    }
}