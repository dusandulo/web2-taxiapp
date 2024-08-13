using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class SetRideTimesModelDto
    {
        public Guid RideId { get; set; }
        public int ArrivalTimeInSeconds { get; set; }
        public int DriverTimeInSeconds { get; set; }
    }
}
