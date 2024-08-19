using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communcation
{
    public interface IRatingStatelessCommunication : IService
    {
        Task<double> CalculateAverageRatingAsync(Guid driverId, List<int> ratings);
    }
}
