using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Common.Models
{
    public class RideDbContext : DbContext
    {
        public RideDbContext(DbContextOptions<RideDbContext> options) : base(options)
        {
        }

        public DbSet<RideModel> Rides { get; set; }
    }
}