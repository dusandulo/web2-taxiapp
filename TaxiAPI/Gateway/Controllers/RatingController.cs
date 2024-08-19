using Communication;
using Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Communcation;
using Common.Models;
using Microsoft.ServiceFabric.Services.Client;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingCommunication _ratingService;

        public RatingsController(IRatingCommunication ratingService)
        {
            _ratingService = ratingService;
        }

        [HttpPost("addrating")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddRating([FromBody] RatingDto ratingDto)
        {
            if (ratingDto == null || ratingDto.RatingValue < 1 || ratingDto.RatingValue > 5)
            {
                return BadRequest("Invalid rating data.");
            }

            var result = await _ratingService.AddRatingAsync(ratingDto);

            if (result)
            {
                return Ok("Rating added successfully.");
            }
            else
            {
                return StatusCode(500, "Failed to add rating.");
            }
        }

        [HttpGet("getaverageratings")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<DriverRatingDto>>> GetAverageRatings()
        {
            try
            {
                var driverRatings = await _ratingService.GetDriverRatingsAsync();
                return Ok(driverRatings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("testresolve")]
        public async Task<IActionResult> TestResolveServiceAddress()
        {
            try
            {
                var serviceName = new Uri("fabric:/TaxiAPI/RatingStateful");
                var partitionKey = new ServicePartitionKey(3);
                var resolver = ServicePartitionResolver.GetDefault();
                var resolvedPartition = await resolver.ResolveAsync(serviceName, partitionKey, CancellationToken.None);

                var addresses = resolvedPartition.Endpoints.Select(e => e.Address).ToList();

                return Ok(addresses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri rešavanju adrese servisa: {ex.Message}");
            }
        }
    }
}
