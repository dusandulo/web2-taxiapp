using Common.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class RegisterUserModelDto
    {
        public class RegisterUserDto
        {
            public IFormFile? Image { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Birthday { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public UserRole Role { get; set; }
        }
    }
}
