using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmoothJorneyAPI.Data;
using SmoothJorneyAPI.DTO;
using SmoothJorneyAPI.Entities;

namespace SmoothJorneyAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TripsController : ControllerBase
    {
        private readonly SmoothJorneyAPIContext _context;

        public TripsController(SmoothJorneyAPIContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDTO dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ID" || c.Type == "Id");
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);

            var trip = new Trips
            {
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TotalBudget = dto.TotalBudget,
                CurrentCost = 0,
                UserId = userId,
                ShareToken = Guid.NewGuid().ToString()
            };

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Trip created!",
                TripId = trip.TripId,
                ShareLink = trip.ShareToken
            });
        }

        [HttpPost("add-item")]
        [Authorize]
        public async Task<IActionResult> AddTripItem([FromBody] AddTripItemDTO dto)
        {
            var trip = await _context.Trips.FindAsync(dto.TripId);
            if (trip == null) return NotFound("Trip not found");

            var item = new TripItem
            {
                TripId = dto.TripId,
                BusinessId = dto.BusinessId,
                ScheduledTime = dto.ScheduledTime
            };
            trip.CurrentCost += dto.EstimatedCost;

            string warningMessage = "";
            if (trip.CurrentCost > trip.TotalBudget)
            {
                warningMessage = "Warning: You have exceeded your budget!";
            }

            _context.TripItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Activity added!", Warning = warningMessage, NewTotal = trip.CurrentCost });
        }

        [HttpGet("shared/{token}")]
        public async Task<IActionResult> GetSharedTrip(string token)
        {
            var trip = await _context.Trips
                .Include(t => t.User)
                .Include(t => t.TripItems!)
                    .ThenInclude(i => i.Business)
                .FirstOrDefaultAsync(t => t.ShareToken == token);

            if (trip == null) return NotFound("Trip not found or link is invalid.");

            var response = new TripResponseDTO
            {
                Title = trip.Title,
                OwnerName = trip.User?.FirstName + " " + trip.User?.LastName,
                TotalBudget = trip.TotalBudget,
                CurrentCost = trip.CurrentCost,
                RemainingBudget = trip.TotalBudget - trip.CurrentCost,
                Activities = trip.TripItems.Select(i => new TripItemResponseDTO
                {
                    BusinessName = i.Business?.Name ?? "Unknown",
                    City = i.Business?.City ?? "-",
                    ScheduledTime = i.ScheduledTime,
                    Cost = 0
                }).OrderBy(a => a.ScheduledTime).ToList()
            };

            return Ok(response);
        }

        [HttpPost("generate-mood")]
        public async Task<IActionResult> GenerateMoodTrip([FromBody] MoodTripRequestDTO request)
        {
            string[] tagsToSearch;

            switch (request.Mood.ToLower())
            {
                case "romantic":
                    tagsToSearch = new[] { "Cozy", "View", "Wine", "Jazz", "Dinner" };
                    break;
                case "party":
                    tagsToSearch = new[] { "Club", "Bar", "Dance", "Cocktail", "Late Night" };
                    break;
                case "adventure":
                    tagsToSearch = new[] { "Hiking", "Sports", "Escape Room", "Activity", "Outdoor" };
                    break;
                case "relax":
                    tagsToSearch = new[] { "Coffee", "Park", "Bookstore", "Spa", "Brunch" };
                    break;
                default:
                    tagsToSearch = new[] { "Food", "Drink" };
                    break;
            }

            int maxUserLevel = CalculateLevelFromBudget(request.Budget);

            var cityBusinesses = await _context.Business
                .Where(b => b.City == request.City)
                .ToListAsync();

            var matchingBusinesses = cityBusinesses
                .Where(b =>
                    ConvertPriceRangeToLevel(b.PriceRange) <= maxUserLevel
                    &&
                    tagsToSearch.Any(tag =>
                        (b.MoodTags != null && b.MoodTags.Contains(tag)) ||
                        (b.Category != null && b.Category.Contains(tag))
                    )
                )
                .ToList();

            if (!matchingBusinesses.Any())
                return NotFound("Sorry, we couldn't find spots for this mood and budget in your city.");
            var random = new Random();
            var selectedSpots = matchingBusinesses.OrderBy(x => random.Next()).Take(2).ToList();

            var suggestion = new TripResponseDTO
            {
                Title = $"A {request.Mood} Journey in {request.City}",
                OwnerName = "SmoothJourney AI",
                TotalBudget = request.Budget,
                Activities = new List<TripItemResponseDTO>()
            };

            if (selectedSpots.Count > 0)
            {
                suggestion.Activities.Add(new TripItemResponseDTO
                {
                    BusinessName = selectedSpots[0].Name,
                    City = selectedSpots[0].City,
                    ScheduledTime = DateTime.Now.AddHours(1),
                    Cost = EstimateCostFromPriceRange(selectedSpots[0].PriceRange)
                });
            }

            if (selectedSpots.Count > 1)
            {
                suggestion.Activities.Add(new TripItemResponseDTO
                {
                    BusinessName = selectedSpots[1].Name,
                    City = selectedSpots[1].City,
                    ScheduledTime = DateTime.Now.AddHours(3),
                    Cost = EstimateCostFromPriceRange(selectedSpots[1].PriceRange)
                });
            }

            suggestion.CurrentCost = suggestion.Activities.Sum(a => a.Cost);
            suggestion.RemainingBudget = suggestion.TotalBudget - suggestion.CurrentCost;

            return Ok(suggestion);
        }

        private int CalculateLevelFromBudget(decimal budget)
        {
            if (budget <= 15) return 1;
            if (budget <= 30) return 2;
            if (budget <= 60) return 3;
            return 4;
        }

        private int ConvertPriceRangeToLevel(string? priceRange)
        {
            if (string.IsNullOrEmpty(priceRange)) return 0;
            return priceRange.Length;
        }

        private decimal EstimateCostFromPriceRange(string? priceRange)
        {
            int level = ConvertPriceRangeToLevel(priceRange);
            switch (level)
            {
                case 1: return 10.00m;
                case 2: return 20.00m;
                case 3: return 40.00m;
                case 4: return 80.00m;
                default: return 15.00m;
            }
        }
    }
}
