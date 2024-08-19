using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class RatingModel
    {
        public Guid Id { get; set; }
        public Guid RideId { get; set; }
        public Guid DriverId { get; set; }
        public Guid PassengerId { get; set; }
        public int RatingValue { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
