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

namespace RatingStateful
{
    internal sealed class RatingStateful : StatefulService, IRatingCommunication
    {
        private readonly IServiceProvider _serviceProvider;

        public RatingStateful(StatefulServiceContext context, IServiceProvider serviceProvider)
            : base(context)
        {
            _serviceProvider = serviceProvider;
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


        public async Task<IEnumerable<RatingModel>> GetRatingsForDriverAsync(Guid driverId)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<RatingDbContext>();
                    return await dbContext.Ratings.Where(r => r.DriverId == driverId).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Error retrieving ratings: {0}", ex.Message);
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
