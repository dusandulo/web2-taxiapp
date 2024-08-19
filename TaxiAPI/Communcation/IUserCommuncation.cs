using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;
using Microsoft.AspNetCore.Http;
using Common.Enums;

namespace Communication
{
    public interface IUserCommunication : IService
    {
        Task Register(UserModel user);

        Task<IEnumerable<UserModel>> GetAllUsers();
        Task<UserModel?> GetUserByEmail(string email);
        Task<string?> Authenticate(string email, string password);
        Task UpdateProfile(UserModel user);
        Task<bool> UpdateVerificationState(Guid userId, VerificationState state);
        Task<IEnumerable<UserModel>> GetUnverifiedDrivers();
        Task<UserModel?> GetUserById(Guid userId);
    }
}
