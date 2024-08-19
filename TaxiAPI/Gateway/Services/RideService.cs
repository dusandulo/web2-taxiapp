using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Enums;
using Communication;
using Common.DTOs;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Communication
{
    public class RideService : IRideCommunication
    {
        private readonly IRideCommunication _rideCommunication;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RideService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _rideCommunication = ServiceProxy.Create<IRideCommunication>(
                new Uri("fabric:/TaxiAPI/RideStateful"), new ServicePartitionKey(2));

            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<RideModel> CreateRideAsync(CreateRideModelDto rideDto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID is not available in token.");
            }

            rideDto.PassengerId = Guid.Parse(userIdClaim.Value);

            return await _rideCommunication.CreateRideAsync(rideDto);
        }

        public async Task<RideModel?> UpdateRideStatusAsync(Guid rideId, RideStatus status)
        {
            return await _rideCommunication.UpdateRideStatusAsync(rideId, status);
        }

        public async Task<IEnumerable<RideModel>> GetAllRidesAsync()
        {
            return await _rideCommunication.GetAllRidesAsync();
        }

        public Task<RideEstimateResponseDto> EstimateRideAsync(string startAddress, string endAddress)
        {
            var random = new Random();
            int estimatedPrice = random.Next(500, 2000);
            int estimatedTime = random.Next(10, 40); 

            var response = new RideEstimateResponseDto
            {
                Price = estimatedPrice,
                Time = estimatedTime
            };

            return Task.FromResult(response);
        }
        public async Task SetRideTimesAsync(Guid rideId, int arrivalTimeInSeconds, int driverTimeInSeconds)
        {
            await _rideCommunication.SetRideTimesAsync(rideId, arrivalTimeInSeconds, driverTimeInSeconds);
        }

        public async Task<RideModel?> GetRideByIdAsync(Guid rideId)
        {
            return await _rideCommunication.GetRideByIdAsync(rideId);
        }

        public async Task<RideModel?> ConfirmRideAsync(Guid rideId, Guid driverId)
        {
            return await _rideCommunication.ConfirmRideAsync(rideId, driverId);
        }

        public async Task<IEnumerable<RideModel>> GetPendingRidesAsync()
        {
            return await _rideCommunication.GetPendingRidesAsync();
        }

        public Task<IEnumerable<RideModel>> GetConfirmedRidesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<RideModel?> FinishRideAsync(Guid rideId, RideStatus status, int driverTimeInSeconds)
        {
            return await _rideCommunication.FinishRideAsync(rideId, status, driverTimeInSeconds);
        }
    }
}