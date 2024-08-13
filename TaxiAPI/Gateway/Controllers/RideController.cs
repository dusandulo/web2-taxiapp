using Common.DTOs;
using Common.Enums;
using Common.Models;
using Communication;
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

        public RideController(IRideCommunication rideService)
        {
            _rideService = rideService;
        }

        [HttpGet("getallrides")]
        public async Task<ActionResult<IEnumerable<RideModel>>> GetAllRides()
        {
            var rides = await _rideService.GetAllRidesAsync();
            return Ok(rides);
        }

        // PUT api/ride/{rideId}/status
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

            var newRide = await _rideService.CreateRideAsync(rideDto);

            // SignalR: Obaveštavanje vozača o novoj vožnji
            var hubContext = HttpContext.RequestServices.GetService<IHubContext<RideHub>>();
            await hubContext.Clients.Group("Drivers").SendAsync("NewRide", newRide);

            return Ok(newRide);
        }
        [HttpPost("settimes")]
        public async Task<IActionResult> SetRideTimes([FromBody] SetRideTimesModelDto setRideTimesDto)
        {
            try
            {
                await _rideService.SetRideTimesAsync(setRideTimesDto.RideId, setRideTimesDto.ArrivalTimeInSeconds, setRideTimesDto.DriverTimeInSeconds);
                return Ok();
            }
            catch (Exception ex)
            {
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

                var hubContext = HttpContext.RequestServices.GetService<IHubContext<RideHub>>();
                await hubContext.Clients.Group(confirmRideDto.RideId.ToString()).SendAsync("RideConfirmed", confirmedRide);

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
    }
}
