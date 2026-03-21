using Microsoft.EntityFrameworkCore;
using SmoothJorneyAPI.Data;
using SmoothJorneyAPI.Entities;
using SmoothJorneyAPI.Interfaces;

namespace SmoothJorneyAPI.Services
{
    public class RecommendationEngine
    {
        private readonly IWeatherService _weather;
        private readonly SmoothJorneyAPIContext _context;

        public RecommendationEngine(SmoothJorneyAPIContext context, IWeatherService weather)
        {
            _context = context;
            _weather = weather;
        }

        public async Task<List<Business>> GetSmartRecommendations(int userId, string city)
        {
            var weatherData = await _weather.GetCurrentWeatherAsync(city);
            bool isRaining = weatherData.Description.ToLower().Contains("rain") ||
                             weatherData.Description.ToLower().Contains("storm");

            var preferredCategories = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.Business.Category)
                .Distinct()
                .ToListAsync();

            var query = _context.Business
                .Include(b => b.Reviews)
                .Where(b => b.City == city && !b.IsSuspectedScam);

            if (isRaining)
            {
                query = query.Where(b => b.Category != "Park"
                                      && b.Category != "Beach"
                                      && b.Category != "Outdoor");
            }

            var businesses = await query.ToListAsync();

            var scoredBusinesses = businesses.Select(b => new
            {
                Business = b,
                Score = CalculateScore(b, preferredCategories)
            })
            .OrderByDescending(x => x.Score)
            .Take(10) // Top 10
            .Select(x => x.Business)
            .ToList();

            return scoredBusinesses;
        }

        private int CalculateScore(Business b, List<string> preferredCategories)
        {
            int score = 0;

            if (b.AverageRating > 0)
            {
                score += (int)(b.AverageRating * 10);
            }


            if (preferredCategories.Contains(b.Category))
            {
                score += 40;
            }

            if (b.Reviews != null)
            {
                score += Math.Min(b.Reviews.Count / 5, 20);
            }

            return score;
        }
    }
}