using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class DriverRatingDto
    {
        public Guid DriverId { get; set; }
        public string DriverName { get; set; }
        public string Email { get; set; } 
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
    }
}
