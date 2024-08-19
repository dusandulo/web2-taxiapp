using System;
using System.Threading.Tasks;
using Common.DTOs;
using Common.Enums;
using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Communication
{
    public interface IRideCommunication : IService
    {
        Task<RideModel> CreateRideAsync(CreateRideModelDto rideDto);
        Task<RideModel?> GetRideByIdAsync(Guid rideId);
        Task<RideModel?> UpdateRideStatusAsync(Guid rideId, RideStatus status);
        Task<IEnumerable<RideModel>> GetAllRidesAsync();
        Task<RideEstimateResponseDto> EstimateRideAsync(string startAddress, string endAddress);
        Task SetRideTimesAsync(Guid rideId, int arrivalTimeInSeconds, int driverTimeInSeconds);
        Task<RideModel?> ConfirmRideAsync(Guid rideId, Guid driverId);
        Task<IEnumerable<RideModel>> GetPendingRidesAsync();
        Task<RideModel?> FinishRideAsync(Guid rideId, RideStatus status, int driverTimeInSeconds);
        Task<IEnumerable<RideModel>> GetConfirmedRidesAsync();

    }
}
