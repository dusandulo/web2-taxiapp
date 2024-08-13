using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class ConfirmRideModelDto
    {
        public Guid RideId { get; set; }
        public Guid DriverId { get; set; }
    }
}
