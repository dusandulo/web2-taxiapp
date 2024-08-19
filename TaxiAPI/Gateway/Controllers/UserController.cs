using Common.Enums;
using Common.Models;
using Common.DTOs;
using Communication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using static Common.DTOs.RegisterUserModelDto;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserCommunication _userCommunication;
        private readonly string _imageFolderPath;

        public UsersController(IUserCommunication userCommunication)
        {
            _userCommunication = userCommunication;
            _imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterUserDto user)
        {
            string? imagePath = null;

            // Kreiraj UserModel instancu i popuni je podacima iz DTO-a
            UserModel newUser = new UserModel
            {
                UserName = user.UserName,
                Email = user.Email,
                Password = user.Password,
                Name = user.Name,
                LastName = user.LastName,
                Birthday = user.Birthday,
                Address = user.Address,
                Role = user.Role
            };

            // Ako postoji slika, snimi je i dodaj putanju do slike u UserModel
            if (user.Image != null && user.Image.Length > 0)
            {
                imagePath = await SaveImage(user.Image);
                newUser.Image = imagePath;
            }

            try
            {
                await _userCommunication.Register(newUser);
                return Ok(new { message = "User registered successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while registering the user.", details = ex.Message });
            }
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            if (!Directory.Exists(_imageFolderPath))
            {
                Directory.CreateDirectory(_imageFolderPath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(_imageFolderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return (fileName);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModelDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            var token = await _userCommunication.Authenticate(loginDto.Email, loginDto.Password);
            if (token == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var user = await _userCommunication.GetUserByEmail(loginDto.Email);
            return Ok(new
            {
                Token = token,
                UserId = user.Id,
                Role = user.Role.ToString()
            });
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UserModel user)
        {
            if (user == null || user.Id == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid user data." });
            }

            var userEmail = User.Identity.Name;
            if (userEmail != user.Email)
            {
                return StatusCode(403, new { message = "You can only update your own profile." });
            }

            try
            {
                await _userCommunication.UpdateProfile(user);
                return Ok(new { message = "Profile updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the profile.", details = ex.Message });
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
                return BadRequest(new { message = "An error occurred while retrieving users.", details = ex.Message });
            }
        }

        [HttpGet("email/{email}")]
        [Authorize]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _userCommunication.GetUserByEmail(email);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
    }
}
