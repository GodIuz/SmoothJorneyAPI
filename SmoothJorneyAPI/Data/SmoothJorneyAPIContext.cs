using Microsoft.EntityFrameworkCore;
using SmoothJorneyAPI.Entities;

namespace SmoothJorneyAPI.Data
{
    public class SmoothJorneyAPIContext : DbContext
    {
        public SmoothJorneyAPIContext (DbContextOptions<SmoothJorneyAPIContext> options)
            : base(options)
        {
        }

        public DbSet<Users> Users { get; set; } = default!;
        public DbSet<RefreshTokens> RefreshTokens { get; set; } = default!;
        public DbSet<Trips> Trips { get; set; }
        public DbSet<TripItem> TripItems { get; set; }
        public DbSet<Business> Business { get; set; } = default!;
        public DbSet<Reviews> Reviews { get; set; } = default!;
        public DbSet<Favorite> Favorites { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Business>()
                .Property(b => b.AverageRating)
                .HasPrecision(18, 2);
        }
    }
}
