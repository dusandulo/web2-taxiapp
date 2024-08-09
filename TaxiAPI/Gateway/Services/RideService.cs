using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Enums;
using Communication;

namespace Communication
{
    public class RideService : IRideCommunication
    {
        private readonly IRideCommunication _rideCommunication;

        public RideService(IConfiguration configuration)
        {
            _rideCommunication = ServiceProxy.Create<IRideCommunication>(
                new Uri("fabric:/TaxiAPI/RideStateful"), new ServicePartitionKey(2));
        }

        public async Task<RideModel> CreateRideAsync(RideModel ride)
        {
            return await _rideCommunication.CreateRideAsync(ride);
        }

        public async Task<RideModel?> GetRideByIdAsync(Guid rideId)
        {
            return await _rideCommunication.GetRideByIdAsync(rideId);
        }

        public async Task<RideModel?> UpdateRideStatusAsync(Guid rideId, RideStatus status)
        {
            return await _rideCommunication.UpdateRideStatusAsync(rideId, status);
        }
    }
}