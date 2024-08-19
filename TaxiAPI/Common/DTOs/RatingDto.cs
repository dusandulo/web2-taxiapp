using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class RatingDto
    {
        public Guid RideId { get; set; }
        public Guid DriverId { get; set; }
        public Guid PassengerId { get; set; }
        public int RatingValue { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
