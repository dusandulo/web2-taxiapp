using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Models;
using Common.DTOs;
using Communcation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Communication;
using System.Collections.Concurrent;

namespace RatingStateful
{
    internal sealed class RatingStateful : StatefulService, IRatingCommunication
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRatingStatelessCommunication _ratingStatelessService;
        private readonly IRideCommunication _rideService;

        public RatingStateful(StatefulServiceContext context, IServiceProvider serviceProvider)
            : base(context)
        {
            _serviceProvider = serviceProvider;
            _ratingStatelessService = ServiceProxy.Create<IRatingStatelessCommunication>(
                new Uri("fabric:/TaxiAPI/RatingStateless"));
            _rideService = ServiceProxy.Create<IRideCommunication>(
                new Uri("fabric:/TaxiAPI/RideStateful"), new ServicePartitionKey(2)) ;
        }

        public async Task<bool> AddRatingAsync(RatingDto ratingDto)
        {
            try
            {
                var ratingModel = new RatingModel
                {
                    Id = Guid.NewGuid(),
                    RideId = ratingDto.RideId,
                    DriverId = ratingDto.DriverId,
                    PassengerId = ratingDto.PassengerId,
                    RatingValue = ratingDto.RatingValue,
                    Comment = ratingDto.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<RatingDbContext>();
                    await dbContext.Ratings.AddAsync(ratingModel);
                    await dbContext.SaveChangesAsync();
                }

                var ratingsDict = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RatingModel>>("ratingsDictionary");

                using (var tx = this.StateManager.CreateTransaction())
                {
                    await ratingsDict.AddOrUpdateAsync(tx, ratingModel.Id, ratingModel, (k, v) => ratingModel);
                    await tx.CommitAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Error adding rating: {0}", ex.Message);
                throw;
            }
        }


        public async Task<List<DriverRatingDto>> GetDriverRatingsAsync()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<RatingDbContext>();
                    var ratings = await dbContext.Ratings.ToListAsync();

                    var groupedRatings = ratings.GroupBy(r => r.DriverId).ToList();

                    var driverRatings = new List<DriverRatingDto>();

                    foreach (var group in groupedRatings)
                    {
                        var averageRating = await _ratingStatelessService.CalculateAverageRatingAsync(group.Key, group.Select(r => r.RatingValue).ToList());

                        var driver = await _rideService.GetDriverByIdAsync(group.Key);

                        var driverRatingDto = new DriverRatingDto
                        {
                            DriverId = driver?.Id ?? group.Key,
                            DriverName = driver?.Name,
                            Email = driver?.Email,
                            AverageRating = averageRating,
                            RatingCount = group.Count(),
                        };

                        driverRatings.Add(driverRatingDto);
                    }

                    return driverRatings;
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Error retrieving driver ratings: {0}", ex.Message);
                throw;
            }
        }


        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var ratingsDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("ratings");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await ratingsDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await ratingsDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
