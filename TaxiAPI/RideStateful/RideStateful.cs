using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Common.Enums;
using Communication;
using Microsoft.EntityFrameworkCore;
using Common.DTOs;

namespace RideStateful
{
    internal sealed class RideStateful : StatefulService, IRideCommunication
    {
        private readonly IServiceProvider _serviceProvider;

        public RideStateful(StatefulServiceContext context, IServiceProvider serviceProvider)
            : base(context)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<RideModel>> GetAllRidesAsync()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<RideDbContext>();
                    return await dbContext.Rides.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Error retrieving all rides: {0}", ex.Message);
                throw;
            }
        }

        public async Task<RideModel> CreateRideAsync(CreateRideModelDto rideDto)
        {
            try
            {
                // Konvertovanje DTO u RideModel
                var ride = new RideModel
                {
                    Id = Guid.NewGuid(), 
                    StartAddress = rideDto.StartAddress,
                    EndAddress = rideDto.EndAddress,
                    Price = rideDto.Price,
                    DriverTimeInSeconds = rideDto.DriverTimeInSeconds,
                    DriverId = rideDto.DriverId ?? Guid.Empty, 
                    PassengerId = rideDto.PassengerId, 
                    Status = RideStatus.Pending
                };

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<RideDbContext>();
                    await dbContext.Rides.AddAsync(ride);
                    await dbContext.SaveChangesAsync();
                }

                var ridesDict = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RideModel>>("rides");

                using (var tx = this.StateManager.CreateTransaction())
                {
                    await ridesDict.AddOrUpdateAsync(tx, ride.Id, ride, (k, v) => v);
                    await tx.CommitAsync();
                }

                return ride;
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Error creating ride: {0}", ex.Message);
                throw;
            }
        }

        public async Task<RideModel?> UpdateRideStatusAsync(Guid rideId, RideStatus status)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<RideDbContext>();
                    var ride = await dbContext.Rides.FindAsync(rideId);
                    if (ride == null)
                    {
                        return null;
                    }

                    ride.Status = status;
                    dbContext.Rides.Update(ride);
                    await dbContext.SaveChangesAsync();

                    var ridesDict = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RideModel>>("rides");

                    using (var tx = this.StateManager.CreateTransaction())
                    {
                        await ridesDict.AddOrUpdateAsync(tx, ride.Id, ride, (k, v) => v);
                        await tx.CommitAsync();
                    }

                    return ride;
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Error updating ride status: {0}", ex.Message);
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

        public Task<RideEstimateResponseDto> EstimateRideAsync(string startAddress, string endAddress)
        {
            throw new NotImplementedException();
        }

        public async Task SetRideTimesAsync(Guid rideId, int arrivalTimeInSeconds, int driverTimeInSeconds)
        {
            try
            {
                var ridesDict = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RideModel>>("rides");

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var ride = await ridesDict.TryGetValueAsync(tx, rideId);

                    if (!ride.HasValue)
                        throw new Exception("Ride not found");

                    var updatedRide = ride.Value;
                    updatedRide.ArrivalTimeInSeconds = arrivalTimeInSeconds;
                    updatedRide.DriverTimeInSeconds = driverTimeInSeconds;

                    await ridesDict.AddOrUpdateAsync(tx, rideId, updatedRide, (k, v) => updatedRide);
                    await tx.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Error setting ride times: {0}", ex.Message);
                throw;
            }
        }

        public async Task<RideModel?> GetRideByIdAsync(Guid rideId)
        {
            try
            {
                var ridesDict = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RideModel>>("rides");

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var ride = await ridesDict.TryGetValueAsync(tx, rideId);
                    return ride.HasValue ? ride.Value : null;
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Error retrieving ride: {0}", ex.Message);
                throw;
            }
        }

        public async Task<RideModel?> ConfirmRideAsync(Guid rideId, Guid driverId)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<RideDbContext>();
                    var ride = await dbContext.Rides.FindAsync(rideId);

                    if (ride == null || ride.Status != RideStatus.Pending)
                    {
                        return null;
                    }

                    ride.Status = RideStatus.Confirmed;
                    ride.DriverId = driverId;

                    dbContext.Rides.Update(ride);
                    await dbContext.SaveChangesAsync();

                    var ridesDict = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RideModel>>("rides");

                    using (var tx = this.StateManager.CreateTransaction())
                    {
                        await ridesDict.AddOrUpdateAsync(tx, ride.Id, ride, (k, v) => ride);
                        await tx.CommitAsync();
                    }

                    return ride;
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Error confirming ride: {0}", ex.Message);
                throw;
            }
        }


        public async Task<IEnumerable<RideModel>> GetPendingRidesAsync()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<RideDbContext>();
                    return await dbContext.Rides.Where(r => r.Status == RideStatus.Pending).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Error retrieving pending rides: {0}", ex.Message);
                throw;
            }
        }
    }
}
