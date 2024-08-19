using Common.DTOs;
using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communcation
{
    public interface IRatingCommunication : IService
    {
        Task<bool> AddRatingAsync(RatingDto ratingDto);
        Task<List<DriverRatingDto>> GetDriverRatingsAsync();
    }
}
