using Common.Enums;
using Common.Models;
using Common.DTOs;
using Communication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserCommunication _userCommunication;

        public UsersController(IUserCommunication userCommunication)
        {
            _userCommunication = userCommunication;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _userCommunication.Register(user);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModelDto loginDto)
        {
            var token = await _userCommunication.Authenticate(loginDto.Email, loginDto.Password);
            if (token == null)
            {
                return Unauthorized();
            }

            return Ok(new { Token = token });
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UserModel user)
        {
            if (user == null || user.Id == Guid.Empty)
            {
                return BadRequest("Invalid user data.");
            }

            var userEmail = User.Identity.Name;
            if (userEmail != user.Email)
            {
                return Forbid("You can only update your own profile.");
            }

            try
            {
                await _userCommunication.UpdateProfile(user);
                return Ok("Profile updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetAllUsers()
        {
            try
            {
                var users = await _userCommunication.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("email/{email}")]
        [Authorize]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _userCommunication.GetUserByEmail(email);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }
    }
}