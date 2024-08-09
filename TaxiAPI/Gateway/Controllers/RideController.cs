using Common.Enums;
using Common.Models;
using Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // POST api/ride/create
        [Authorize(Roles = "User")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateRide([FromBody] RideModel ride)
        {
            if (ride == null)
            {
                return BadRequest("Invalid ride data.");
            }

            try
            {
                var createdRide = await _rideService.CreateRideAsync(ride);
                return CreatedAtAction(nameof(GetRideById), new { rideId = createdRide.Id }, createdRide);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET api/ride/{rideId}
        [Authorize]
        [HttpGet("{rideId}")]
        public async Task<IActionResult> GetRideById(Guid rideId)
        {
            try
            {
                var ride = await _rideService.GetRideByIdAsync(rideId);
                if (ride == null)
                {
                    return NotFound();
                }
                return Ok(ride);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
    }
}
