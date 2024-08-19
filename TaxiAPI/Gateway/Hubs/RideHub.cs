using Communication;
using Gateway.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

public class RideHub : Hub<IRideHub>
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        Console.WriteLine($"Connection {Context.ConnectionId} joined group {groupName}");
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task UpdateRideTime(int arrivalTimeInSeconds)
    {
        await Clients.Caller.UpdateRideTime(arrivalTimeInSeconds);
    }
    public async Task FinishRide(Guid rideId)
    {
        await Clients.Group(rideId.ToString()).RideFinished(rideId);
        Console.WriteLine($"Ride {rideId} has been finished.");
    }

    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        if (user?.Identity.IsAuthenticated ?? false)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"User {userId} joined group {userId} with connection ID {Context.ConnectionId}.");
            }
        }
        else
        {
            Console.WriteLine("Unauthorized connection attempt.");
        }

        await base.OnConnectedAsync();
    }
}
