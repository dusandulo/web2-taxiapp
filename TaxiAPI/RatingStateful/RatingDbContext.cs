using Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatingStateful
{
    public class RatingDbContext : DbContext
    {
        public RatingDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<RatingModel> Ratings { get; set; }
    }
}
