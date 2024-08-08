using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;

namespace Communication
{
    public interface IUserCommunication : IService
    {
        Task Register(UserModel user);

        Task<IEnumerable<UserModel>> GetAllUsers();
        Task<UserModel?> GetUserByEmail(string email);
        Task<string?> Authenticate(string email, string password);
        Task UpdateProfile(UserModel user);
    }
}
