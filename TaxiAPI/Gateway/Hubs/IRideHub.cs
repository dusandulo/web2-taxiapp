using Common.Models;
using System.Threading.Tasks;

namespace Gateway.Hubs
{
    public interface IRideHub
    {
        Task NewRide(RideModel ride);
        Task RideConfirmed(RideModel ride);
        Task UpdateRideTime(int arrivalTimeInSeconds);
        Task RideFinished(Guid rideId);
    }
}
