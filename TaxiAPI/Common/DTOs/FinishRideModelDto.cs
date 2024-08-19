using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class FinishRideModelDto
    {
        [Required]
        public Guid RideId { get; set; }

        [Required]
        public int RideTimeInSeconds { get; set; }
    }
}
