using Communication;
using Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Communcation;
using Common.Models;

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

        [HttpGet("getratings/{driverId}")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<IEnumerable<RatingResponseDto>>> GetRatingsForDriver(Guid driverId)
        {
            var ratings = await _ratingService.GetRatingsForDriverAsync(driverId);
            var ratingDtos = new List<RatingResponseDto>();

            foreach (var rating in ratings)
            {
                ratingDtos.Add(new RatingResponseDto
                {
                    Id = rating.Id,
                    RideId = rating.RideId,
                    DriverId = rating.DriverId,
                    PassengerId = rating.PassengerId,
                    RatingValue = rating.RatingValue,
                    Comment = rating.Comment,
                    CreatedAt = rating.CreatedAt
                });
            }

            return Ok(ratingDtos);
        }
    }
}
