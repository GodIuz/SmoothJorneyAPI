using Microsoft.EntityFrameworkCore;
using SmoooothJourneyApi.Entities;
using SmoothJorneyAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public DbSet<TripCollaborator> TripCollaborators { get; set; }
        public DbSet<Business> Business { get; set; } = default!;
        public DbSet<BusinessImage> BusinessImages { get; set; } = default!;
        public DbSet<BusinessReport> BusinessReports { get; set; } = default!;
        public DbSet<Reviews> Reviews { get; set; } = default!;
        
        public DbSet<Favorite> Favorites { get; set; } = default!;
            
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Trips>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TripCollaborator>()
                .HasOne(tc => tc.User)
                .WithMany()
                .HasForeignKey(tc => tc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TripCollaborator>()
                .HasOne(tc => tc.Trip)
                .WithMany(t => t.Collaborators)
                .HasForeignKey(tc => tc.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
