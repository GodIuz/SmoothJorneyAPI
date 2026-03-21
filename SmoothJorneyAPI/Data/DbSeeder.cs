using Microsoft.EntityFrameworkCore;
using SmoothJorneyAPI.Entities;
using SmoothJorneyAPI.Services;

namespace SmoothJorneyAPI.Data
{
    public class DbSeeder
    {
        private readonly SmoothJorneyAPIContext _context;
        private readonly Argon2PasswordHasher _hasher;

        public DbSeeder(SmoothJorneyAPIContext context, Argon2PasswordHasher hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        public async Task SeedAsync()
        {
            if (await _context.Users.CountAsync() > 5) return;

            var random = new Random();
            string[] posComments = { "Απλά τέλειο!", "Η καλύτερη εμπειρία στην Ελλάδα.", "Πολύ καθαρό, ευγενικό προσωπικό.", "Value for money!", "Αξίζει την επίσκεψη." };
            string[] negComments = { "Μείνετε μακριά!", "Απάτη, μας χρέωσαν διπλά.", "Πολύ βρώμικο και κακό σέρβις.", "Απαράδεκτη συμπεριφορά.", "Δεν θα ξαναπατήσω." };
            string[] midComments = { "Καλό αλλά ακριβό.", "Μέτριο φαγητό, ωραία θέα.", "Ήταν οκ, τίποτα το ιδιαίτερο.", "Λίγη αναμονή παραπάνω." };
            var users = new List<Users>();
            for (int i = 1; i <= 25; i++)
            {
                var securityData = _hasher.HashPassword("Password123!");
                var randomDays = random.Next(18 * 365, 60 * 365);
                var birthDate = DateTime.UtcNow.AddDays(-randomDays);

                users.Add(new Users
                {
                    UserName = $"Traveler_{i}",
                    Email = $"user{i}@example.com",
                    FirstName = "User_" + i,
                    LastName = "Smooth",
                    PasswordHash = securityData.Hash,
                    PasswordSalt = securityData.Salt,
                    Gender = random.Next(2) == 0  ? "Άνδρας" : "Γυναίκα",
                    Role = "User",
                    EmailConfirmed = true,
                    Country = "Ελλάδα",
                    City = random.Next(2) == 0 ? "Αθήνα" : "Θεσσαλονίκη",
                    DateOfBirth = DateOnly.FromDateTime(birthDate)
                });
            }
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
            var businesses = await _context.Business.ToListAsync();

            foreach (var user in users)
            {
                int totalUserReviews = random.Next(10, 1001);
                var userReviews = new List<Reviews>();

                for (int j = 0; j < totalUserReviews; j++)
                {
                    var biz = businesses[random.Next(businesses.Count)];

                    int rating;
                    string comment;

                    if (biz.BusinessId % 3 == 0) 
                    {
                        rating = random.Next(4, 6);
                        comment = posComments[random.Next(posComments.Length)];
                    }
                    else if (biz.BusinessId % 5 == 0) 
                    {
                        rating = random.Next(1, 3);
                        comment = negComments[random.Next(negComments.Length)];
                    }
                    else 
                    {
                        rating = random.Next(2, 5);
                        comment = midComments[random.Next(midComments.Length)];
                    }

                    userReviews.Add(new Reviews
                    {
                        UserId = user.UserId,
                        BusinessId = biz.BusinessId,
                        Rating = rating,
                        Content = comment,
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(365))
                    });

                    if (userReviews.Count >= 500)
                    {
                        _context.Reviews.AddRange(userReviews);
                        userReviews.Clear();
                    }
                }
                _context.Reviews.AddRange(userReviews);
            }

            await _context.SaveChangesAsync();
            await SyncBusinessStats();
        }

        private async Task SyncBusinessStats()
        {
            var businesses = await _context.Business.ToListAsync();
            foreach (var biz in businesses)
            {
                var bizReviews = _context.Reviews.Where(r => r.BusinessId == biz.BusinessId);
                if (await bizReviews.AnyAsync())
                {
                    var avg = (decimal)await bizReviews.AverageAsync(r => r.Rating);
                    biz.AverageRating = avg;
                    if (avg < 2.0m) biz.IsSuspectedScam = true;
                    if (avg > 4.5m && await bizReviews.CountAsync() < 30) biz.IsHiddenGem = true;
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}