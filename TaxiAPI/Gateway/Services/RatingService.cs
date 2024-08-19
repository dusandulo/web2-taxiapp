﻿using Common.DTOs;
using Common.Models;
using Communcation;
using Communication;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric;

namespace Gateway.Services
{
    public class RatingService : IRatingCommunication
    {
        private readonly IRatingCommunication _ratingCommunication;

        public RatingService(IConfiguration configuration)
        {
            _ratingCommunication = ServiceProxy.Create<IRatingCommunication>(
                new Uri("fabric:/TaxiAPI/RatingStateful"), new ServicePartitionKey(1));
        }

        public async Task<bool> AddRatingAsync(RatingDto ratingDto)
        {
            return await _ratingCommunication.AddRatingAsync(ratingDto);
        }

        public async Task<IEnumerable<RatingModel>> GetRatingsForDriverAsync(Guid driverId)
        {
            return await _ratingCommunication.GetRatingsForDriverAsync(driverId);
        }
    }
}
