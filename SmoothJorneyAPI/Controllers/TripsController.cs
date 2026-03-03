using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmoothJorneyAPI.Data;
using SmoothJorneyAPI.DTO;
using SmoothJorneyAPI.Entities;

namespace SmoothJorneyAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
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
                Message = "Το ταξίδι δημιουργήθηκε!",
                TripId = trip.TripId,
                ShareLink = trip.ShareToken
            });
        }

        [HttpPost("add-item")]
        [Authorize]
        public async Task<IActionResult> AddTripItem([FromBody] AddTripItemDTO dto)
        {
            var trip = await _context.Trips.FindAsync(dto.TripId);
            if (trip == null) return NotFound("Το ταξίδι δεν βρέθηκε.");

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
                warningMessage = "Warning: Έχετε ξεπεράσει τον προϋπολογισμό σας!";
            }

            _context.TripItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Η διαστηριότητα προστέθηκε!", Warning = warningMessage, NewTotal = trip.CurrentCost });
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

        [HttpPost("create-manual")]
        [Authorize]
        public async Task<IActionResult> CreateManualTrip([FromBody] CreateManualTripDTO dto)
        {
            var userId = int.Parse(User.FindFirst("ID")?.Value ?? "0");

            var trip = new Trips
            {
                UserId = userId,
                Title = dto.Title,
                Description = $"Ταξιδι στην {dto.City}",
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TotalBudget = dto.TotalBudget,
                CurrentCost = 0,
                ShareToken = Guid.NewGuid().ToString()
            };

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Το ταξίδι δημιουργήθηκε με επιτυχία!", TripId = trip.TripId });
        }

        [HttpPost("{tripId}/add-item")]
        [Authorize]
        public async Task<IActionResult> AddTripItem(int tripId, [FromBody] AddTripItemDTO dto)
        {
            var userId = int.Parse(User.FindFirst("ID")?.Value ?? "0");
            var trip = await _context.Trips
                .Include(t => t.TripItems)
                .FirstOrDefaultAsync(t => t.TripId == tripId && t.UserId == userId);

            if (trip == null)
            { 
                return NotFound("Δεν βρέθηκε το ταξίδι ή η πρόσβαση απορρίφθηκε."); 
            }

            if (dto.ScheduledTime < trip.StartDate || dto.ScheduledTime > trip.EndDate)
            {
                return BadRequest($"Η ημερομηνία δραστηριότητας πρέπει να είναι μεταξύ {trip.StartDate:dd/MM} και {trip.EndDate:dd/MM}.");
            }

            var newItem = new TripItem
            {
                TripId = tripId,
                Title = dto.Title,
                Description = dto.Description,
                ScheduledTime = dto.ScheduledTime,
                EstimatedCost = dto.Cost,
                IsCompleted = false
            };

            trip.CurrentCost += dto.Cost;

            _context.TripItems.Add(newItem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Activity added!",
                NewCurrentCost = trip.CurrentCost,
                RemainingBudget = trip.TotalBudget - trip.CurrentCost
            });
        }
    }
}
