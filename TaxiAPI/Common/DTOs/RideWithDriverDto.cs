using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class RideWithDriverDto
    {
        public Guid Id { get; set; }
        public string StartAddress { get; set; }
        public string EndAddress { get; set; }
        public decimal Price { get; set; }
        public int DriverTimeInSeconds { get; set; }
        public int ArrivalTimeInSeconds { get; set; }
        public RideStatus Status { get; set; }
        public DriverDto? Driver { get; set; }
    }
}
