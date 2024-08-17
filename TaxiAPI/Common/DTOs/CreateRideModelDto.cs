using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class CreateRideModelDto
    {
        public string StartAddress { get; set; } = string.Empty;
        public string EndAddress { get; set; } = string.Empty;
        public int Price { get; set; }
        public int ArrivalTimeInSeconds { get; set; }
        public Guid? DriverId { get; set; } 
        public Guid PassengerId { get; set; }  
    }
}
