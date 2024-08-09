using Common.Enums;
using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class RideModel
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public Guid DriverId { get; set; }
        [DataMember]
        public Guid PassengerId { get; set; }
        [DataMember]
        public string StartAddress { get; set; } = string.Empty;
        [DataMember]
        public string EndAddress { get; set; } = string.Empty;
        [DataMember]
        public int Price { get; set; }
        [DataMember]
        public int DriverTimeInSeconds { get; set; }
        [DataMember]
        public int ArrivalTimeInSeconds { get; set; } = 0;
        [DataMember]
        public RideStatus Status { get; set; } = RideStatus.Pending;
    }
}
