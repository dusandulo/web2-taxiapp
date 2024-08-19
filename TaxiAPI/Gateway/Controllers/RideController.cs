using Azure.Core;
using Common.DTOs;
using Common.Enums;
using Common.Models;
using Communication;
using Gateway.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RideController : ControllerBase
    {
        private readonly IRideCommunication _rideService;
        private readonly IHubContext<RideHub, IRideHub> _rideHub;

        public RideController(IRideCommunication rideService, IHubContext<RideHub, IRideHub> rideHub)
        {
            _rideService = rideService;
            _rideHub = rideHub;
        }

        [HttpGet("getallrides")]
        public async Task<ActionResult<IEnumerable<RideModel>>> GetAllRides([FromQuery] Guid? userId)
        {
            try
            {
                if (userId == null)
                {
                    return BadRequest("User ID is required.");
                }

                var rides = await _rideService.GetAllRidesAsync();

                var filteredRides = rides.Where(ride => ride.PassengerId == userId || ride.DriverId == userId).ToList();

                return Ok(filteredRides);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("getallridesadmin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<RideWithDriverDto>>> GetAllRides()
        {
            try
            {
                var rides = await _rideService.GetAllRidesAsync();

                var ridesWithDriverInfo = new List<RideWithDriverDto>();

                foreach (var ride in rides)
                {
                    var driver = await _rideService.GetDriverByIdAsync(ride.DriverId);
                    var rideWithDriver = new RideWithDriverDto
                    {
                        Id = ride.Id,
                        StartAddress = ride.StartAddress,
                        EndAddress = ride.EndAddress,
                        Price = ride.Price,
                        DriverTimeInSeconds = ride.DriverTimeInSeconds,
                        ArrivalTimeInSeconds = ride.ArrivalTimeInSeconds,
                        Status = ride.Status,
                        Driver = driver != null ? new DriverDto
                        {
                            Id = driver.Id,
                            Name = driver.Name,
                            Email = driver.Email,
                        } : null
                    };

                    ridesWithDriverInfo.Add(rideWithDriver);
                }

                return Ok(ridesWithDriverInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Driver")]
        [HttpPut("{rideId}/status")]
        public async Task<IActionResult> UpdateRideStatus(Guid rideId, [FromBody] RideStatus status)
        {
            try
            {
                var updatedRide = await _rideService.UpdateRideStatusAsync(rideId, status);
                if (updatedRide == null)
                {
                    return NotFound();
                }
                return Ok(updatedRide);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("estimate")]
        public async Task<ActionResult<RideEstimateResponseDto>> EstimateRide([FromBody] RideEstimateDto dto)
        {
            var estimate = await _rideService.EstimateRideAsync(dto.StartAddress, dto.EndAddress);
            return Ok(estimate);
        }

        [HttpPost("createride")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<RideModel>> CreateRide([FromBody] CreateRideModelDto rideDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newRide = await _rideService.CreateRideAsync(rideDto);

                try
                {
                    await _rideHub.Clients.Group("Drivers").NewRide(newRide);
                    Console.WriteLine("New ride notification sent to drivers.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending new ride notification: {ex.Message}");
                }

                return Ok(newRide);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating ride: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{rideId}")]
        public async Task<IActionResult> GetRide(Guid rideId)
        {
            var ride = await _rideService.GetRideByIdAsync(rideId);

            if (ride == null)
                return NotFound("Ride not found");

            return Ok(ride);
        }

        [Authorize(Roles = "Driver")]
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmRide([FromBody] ConfirmRideModelDto confirmRideDto)
        {
            if (confirmRideDto == null || confirmRideDto.RideId == Guid.Empty)
            {
                return BadRequest("Invalid ride data.");
            }

            try
            {
                var confirmedRide = await _rideService.ConfirmRideAsync(confirmRideDto.RideId, confirmRideDto.DriverId);

                if (confirmedRide == null)
                {
                    return NotFound("Ride not found or already confirmed.");
                }

                // Poziv metode RideConfirmed na klijentskoj strani kroz IRideHub
                await _rideHub.Clients.Group(confirmedRide.Id.ToString())
                                      .RideConfirmed(confirmedRide);

                Console.WriteLine($"RideConfirmed sent to passenger: {confirmedRide.PassengerId}");

                return Ok(confirmedRide);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Driver")]
        [HttpGet("pendingrides")]
        public async Task<ActionResult<IEnumerable<RideModel>>> GetPendingRides()
        {
            try
            {
                var pendingRides = await _rideService.GetPendingRidesAsync();
                return Ok(pendingRides);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Driver")]
        [HttpPost("{rideId}/finish")]
        public async Task<IActionResult> FinishRide([FromBody] FinishRideModelDto finishRideDto)
        {
            if (finishRideDto == null || finishRideDto.RideId == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid ride data." });
            }
            try
            {
                var ride = await _rideService.GetRideByIdAsync(finishRideDto.RideId);
                if (ride == null)
                {
                    return NotFound("Ride not found.");
                }

                var updatedRide = await _rideService.FinishRideAsync(finishRideDto.RideId, RideStatus.Finished, finishRideDto.RideTimeInSeconds);

                await _rideHub.Clients.Group(ride.Id.ToString()).RideFinished(ride.Id);
                Console.WriteLine($"RideFinished sent to passenger: {ride.PassengerId} and driver: {ride.DriverId}");


                return Ok(updatedRide);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
