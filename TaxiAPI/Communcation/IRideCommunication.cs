using System;
using System.Threading.Tasks;
using Common.Enums;
using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Communication
{
    public interface IRideCommunication : IService
    {
        Task<RideModel> CreateRideAsync(RideModel ride);
        Task<RideModel?> GetRideByIdAsync(Guid rideId);
        Task<RideModel?> UpdateRideStatusAsync(Guid rideId, RideStatus status);
    }
}
