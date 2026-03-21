using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmoothJorneyAPI.Data;
using SmoothJorneyAPI.DTO;
using SmoothJorneyAPI.Entities;
using SmoothJorneyAPI.Interfaces;
using SmoothJorneyAPI.Services;
using System.Security.Claims;
using System.Text.Json;

namespace SmoothJorneyAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly SmoothJorneyAPIContext _context;
        private readonly IWeatherService _weatherService;
        private readonly IAiService _aiService;

        public TripsController(
            SmoothJorneyAPIContext context,
            IWeatherService weatherService,
            IAiService aiService)
        {
            _context = context;
            _weatherService = weatherService;
            _aiService = aiService;
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
                City = dto.City,
                UserId = userId,
            };

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Το ταξίδι δημιουργήθηκε!"
            });
        }

        [HttpPost("add-item")]
        [Authorize]
        public async Task<IActionResult> AddTripItem([FromBody] AddTripItemDTO dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ID" || c.Type == "Id");
            if (userIdClaim == null) return Unauthorized("Δεν βρέθηκε ID χρήστη στο Token.");
            var userId = int.Parse(userIdClaim.Value);
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.TripId == dto.TripId && t.UserId == userId);

            if (trip == null)
                return NotFound("Το ταξίδι δεν βρέθηκε ή δεν έχετε δικαίωμα πρόσβασης.");

            var businessExists = await _context.Business.AnyAsync(b => b.BusinessId == dto.BusinessId);
            if (!businessExists)
            {
                return NotFound("Η επιχείρηση που προσπαθείτε να προσθέσετε δεν βρέθηκε στη βάση.");
            }

            if (dto.ScheduledTime < trip.StartDate || dto.ScheduledTime > trip.EndDate)
            {
                return BadRequest($"Η ημερομηνία δραστηριότητας πρέπει να είναι μεταξύ {trip.StartDate:dd/MM} και {trip.EndDate:dd/MM}.");
            }

            var item = new TripItem
            {
                TripId = dto.TripId,
                BusinessId = dto.BusinessId,
                ScheduledTime = dto.ScheduledTime,
                Title = dto.Title,
                Description = dto.Description
            };

             

            _context.TripItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Η δραστηριότητα προστέθηκε!"
            });
        }

        [HttpPost("generate-mood")]
        [Authorize]
        public async Task<IActionResult> GenerateMoodTrip([FromBody] MoodTripRequestDTO request)
        {
            try
            {
                var myBusinesses = await _context.Business
                    .Where(b => b.City.ToLower() == request.City.ToLower())
                    .ToListAsync();
                string businessContext = string.Join(", ", myBusinesses.Select(b => $"{b.Name} ({b.Category})"));
                var aiPlanJson = await _aiService.GetDetailedTripPlanAsync(request, businessContext);

                Console.WriteLine("DEBUG AI RESPONSE: " + aiPlanJson);

                if (string.IsNullOrEmpty(aiPlanJson) || aiPlanJson.StartsWith("Error") || aiPlanJson.StartsWith("Exception"))
                {
                    return StatusCode(500, $"AI Service Error: {aiPlanJson}");
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                TripPlanDTO plan;

                try
                {
                    plan = JsonSerializer.Deserialize<TripPlanDTO>(aiPlanJson, options);
                }
                catch (JsonException jex)
                {
                    return BadRequest(new
                    {
                        error = "Το AI δεν έστειλε καθαρό JSON.",
                        details = jex.Message,
                        rawAiResponse = aiPlanJson
                    });
                }

                if (plan == null || plan.Days == null) return BadRequest("Το πλάνο είναι κενό.");

                foreach (var day in plan.Days)
                {
                    foreach (var act in day.Activities)
                    {
                        var match = myBusinesses.FirstOrDefault(b =>
                            b.Name.ToLower().Trim() == act.Title.ToLower().Trim() ||
                            act.Title.ToLower().Contains(b.Name.ToLower().Trim()));

                        if (match != null)
                        {
                            act.BusinessId = match.BusinessId;
                        }
                    }
                }

                return Ok(new { plan });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Error: {ex.Message}");
            }
        }

        [HttpPost("save-ai-trip")]
        [Authorize]
        public async Task<IActionResult> SaveAiTrip([FromBody] SaveAiTripDTO dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ID" || c.Type == "Id");
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var trip = new Trips
                {
                    Title = dto.Title,
                    City = dto.City,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    TotalBudget = dto.TotalBudget,
                    UserId = userId,
                    Mood = dto.Mood ?? "Relax",
                    Description = dto.Description ?? $"AI Ταξίδι: {dto.City}"
                };

                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();

                var cityBusinesses = await _context.Business
                                    .Where(b => b.City.ToLower() == dto.City.ToLower())
                                    .ToListAsync();

                foreach (var dayDto in dto.Days)
                {
                    foreach (var actDto in dayDto.Activities)
                    {
                        var matchedBusiness = cityBusinesses
                            .FirstOrDefault(b => b.Name.ToLower().Trim() == actDto.Title.ToLower().Trim());
                        var tripItem = new TripItem
                        {
                            TripId = trip.TripId,
                            Title = actDto.Title,
                            Description = actDto.Description,
                            ScheduledTime = trip.StartDate.AddDays(dayDto.Day - 1),
                            BusinessId = matchedBusiness?.BusinessId
                        };

                        _context.TripItems.Add(tripItem);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = "Το ταξίδι και οι επιχειρήσεις αποθηκεύτηκαν!", TripId = trip.TripId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Internal Error: {ex.Message}");
            }
        }

        [HttpPost("create-manual")]
        [Authorize]
        public async Task<IActionResult> CreateManualTrip([FromBody] CreateManualTripDTO dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ID" || c.Type == "Id");
            if (userIdClaim == null) return Unauthorized("Δεν βρέθηκε ID χρήστη στο Token.");

            var userId = int.Parse(userIdClaim.Value);

            var trip = new Trips
            {
                UserId = userId,
                Title = dto.Title ?? "",
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TotalBudget = dto.TotalBudget,
                City = dto.City,
                Description = dto.Description ?? $"Ταξίδι στην {dto.City}",
                Mood = dto.Mood ?? "Relax"
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
            };

            _context.TripItems.Add(newItem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Activity added!"
            });
        }

        [HttpGet("city/{cityName}")]
        [Authorize]
        public async Task<IActionResult> GetBusinessesByCity(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return BadRequest("Το όνομα της πόλης δεν μπορεί να είναι κενό.");
            }

            var businesses = await _context.Business
                .Where(b => b.City != null && b.City.ToLower() == cityName.ToLower())
                .ToListAsync();

            if (!businesses.Any())
            {
                return Ok(new List<object>());
            }

            return Ok(businesses);
        }

        [HttpGet("my-trips")]
        [Authorize]
        public async Task<IActionResult> GetMyTrips()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ID" || c.Type == "Id" || c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("Δεν βρέθηκε ID χρήστη στο Token.");

            int userId = int.Parse(userIdClaim.Value);
            var rawTrips = await _context.Trips
                .Where(t => t.UserId == userId)
                .Include(t => t.TripItems)
                    .ThenInclude(ti => ti.Business)
                .OrderByDescending(t => t.StartDate)
                .AsSplitQuery()
                .ToListAsync();

            var uniqueTrips = rawTrips.DistinctBy(t => t.TripId).ToList();

            var trips = uniqueTrips.Select(t => new
            {
                id = t.TripId,
                destination = !string.IsNullOrEmpty(t.Title) ? t.Title : (t.City ?? "Άγνωστος Προορισμός"),
                startDate = t.StartDate,
                endDate = t.EndDate,
                mood = t.Mood ?? "Δεν ορίστηκε",
                totalBudget = t.TotalBudget,

                imageUrl = t.TripItems.Where(ti => ti.Business != null && ti.Business.ImageUrl != null)
                                      .Select(ti => ti.Business.ImageUrl)
                                      .FirstOrDefault() ?? "",

                activities = t.TripItems.Select(ti => new
                {
                    id = ti.TripItemId,
                    day = (ti.ScheduledTime.Date - t.StartDate.Date).Days + 1,
                    time = ti.ScheduledTime.ToString("HH:mm"),
                    businessId = ti.BusinessId,
                    businessName = ti.Business != null ? ti.Business.Name : ti.Title,
                    businessCategory = ti.Business != null ? ti.Business.Category : "Custom",
                    imageUrl = ti.Business != null ? ti.Business.ImageUrl : "",
                    notes = ti.Description
                }).OrderBy(a => a.day).ThenBy(a => a.time).ToList()
            }).ToList();

            return Ok(trips);
        }
    }
}
