using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Models;
using Communication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace UserStateful
{
    internal sealed class UserStateful : StatefulService, IUserCommunication
    {
        private readonly IServiceProvider _serviceProvider;

        public UserStateful(StatefulServiceContext context, IServiceProvider serviceProvider)
            : base(context)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<string?> Authenticate(string email, string password)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                    return await dbContext.Users.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Error retrieving users from database: {0}", ex.Message);
                throw;
            }
        }

        public async Task<UserModel?> GetUserByEmail(string email)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                    return await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Error retrieving user from database: {0}", ex.Message);
                throw;
            }
        }

        public async Task Register(UserModel user)
        {
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                throw new ArgumentException("Email and password are required.");
            }

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                    await dbContext.Users.AddAsync(user);
                    await dbContext.SaveChangesAsync();
                }

                var usersDict = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("users");

                using (var tx = this.StateManager.CreateTransaction())
                {
                    await usersDict.AddOrUpdateAsync(tx, user.Id, user, (k, v) => v);
                    await tx.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Error saving user to database: {0}", ex.Message);
                throw;
            }
        }

        public async Task UpdateProfile(UserModel user) 
        {
            if (user == null || user.Id == Guid.Empty)
            {
                throw new ArgumentException("User and user ID are required.");
            }

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                    var existingUser = await dbContext.Users.FindAsync(user.Id);

                    if (existingUser == null)
                    {
                        throw new InvalidOperationException("User not found.");
                    }

                    existingUser.UserName = user.UserName;
                    existingUser.Email = user.Email;
                    existingUser.Password = !string.IsNullOrEmpty(user.Password) ? BCrypt.Net.BCrypt.HashPassword(user.Password) : existingUser.Password;
                    existingUser.Name = user.Name;
                    existingUser.LastName = user.LastName;
                    existingUser.Birthday = user.Birthday;
                    existingUser.Address = user.Address;
                    existingUser.Role = user.Role;
                    existingUser.Image = user.Image;
                    existingUser.VerificationState = user.VerificationState;

                    dbContext.Users.Update(existingUser);
                    await dbContext.SaveChangesAsync();
                }

                var usersDict = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("users");

                using (var tx = this.StateManager.CreateTransaction())
                {
                    await usersDict.AddOrUpdateAsync(tx, user.Id, user, (k, v) => v);
                    await tx.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Error updating user profile: {0}", ex.Message);
                throw;
            }
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}