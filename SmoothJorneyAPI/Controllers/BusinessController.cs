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
    public class BusinessController : ControllerBase
    {
        private readonly SmoothJorneyAPIContext _context;

        public BusinessController(SmoothJorneyAPIContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Business>>> GetBusinesses()
        {
            return await _context.Business
                .Include(b => b.Reviews)
                .ThenInclude(r => r.User) 
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BusinessDetailDTO>> GetBusinessById(int id)
        {
            var business = await _context.Business
                .Include(b => b.Photos)
                .FirstOrDefaultAsync(b => b.BusinessId == id);

            if (business == null) return NotFound("Η Επιχείρηση δεν βρέθηκε");

            var detailDto = new BusinessDetailDTO
            {
                Id = business.BusinessId,
                Name = business.Name ?? "",
                Category = business.Category ?? "",
                CategoryType = business.CategoryType ?? "",
                Description = business.Description ?? "",
                Country = business.Country ?? "",
                City = business.City ?? "",
                AverageRating = business.AverageRating,
                PriceLevel = business.PriceLevel,
                PriceRange = business.PriceRange ?? "",
                IsHiddenGem = business.IsHiddenGem,
                MoodTags = business.MoodTags ?? "",
                Address = business.Address ?? "",
                Phone = business.Phone ?? "N/A",
                IsSuspectedScam = business.IsSuspectedScam,
                ImageUrl = business.ImageUrl,
                GalleryPhotos = business.Photos.Select(p => p.Url).ToList()
            };

            return Ok(detailDto);
        }

        [HttpPost("create-business")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBusiness([FromBody] CreateBusinessDTO dto)
        {
            var business = new Business
            {
                Name = dto.Name,
                Category = dto.Category,
                CategoryType = dto.CategoryType,
                MoodTags = dto.MoodTags,
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country,
                Phone = dto.Phone,
                PriceRange = dto.PriceRange,
                PriceLevel = dto.PriceLevel,
                AverageRating = 0,
                Description = dto.Description,
                IsHiddenGem = dto.IsHiddenGem,
                IsSuspectedScam = dto.IsSuspectedScam,
                CreateAt = DateTime.Now,
                ImageUrl = dto.ImageUrl
            };

            _context.Business.Add(business);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Η επιχείρηση δημιουργήθηκε επιτυχώς!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/set-cover")]
        public async Task<IActionResult> SetCoverPhoto(int id, [FromBody] string photoPath)
        {
            var business = await _context.Business.FindAsync(id);
            if (business == null) return NotFound();

            business.ImageUrl = photoPath;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cover photo updated!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/add-gallery-photo")]
        public async Task<IActionResult> AddGalleryPhoto(int id, [FromBody] string photoPath)
        {
            var business = await _context.Business.FindAsync(id);
            if (business == null) return NotFound();

            var newPhoto = new BusinessPhoto
            {
                Url = photoPath,
                BusinessId = id
            };

            _context.Photos.Add(newPhoto);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Photo added to gallery!" });
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Business>>> SearchByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Παρακαλώ δώστε ένα όνομα για αναζήτηση.");
            }

            var businesses = await _context.Business
                .Where(b => b.Name.Contains(name))
                .OrderBy(b => b.Name)
                .ToListAsync();

            return Ok(businesses);
        }

        [HttpGet("discover")]
        public async Task<ActionResult<IEnumerable<Business>>> GetDiscoverBusinesses(
             [FromQuery] string? sortBy,
             [FromQuery] bool isAscending = true,
             [FromQuery] decimal? userBudget = null,
             [FromQuery] bool hiddenGemsMode = false,
             [FromQuery] bool showScams = false
         )
        {
            var query = _context.Business.AsQueryable();
            query = query.Where(b => b.Category != "Hidden Gem");
            var businessesList = await query.ToListAsync();

            if (userBudget.HasValue)
            {
                int maxUserLevel = CalculateLevelFromBudget(userBudget.Value);
                businessesList = businessesList
                    .Where(b => ConvertPriceRangeToLevel(b.PriceRange) <= maxUserLevel)
                    .ToList();
            }

            if (hiddenGemsMode)
            {
                query = query.Where(b => b.Category == "Hidden Gem");
            }
            else
            {
                query = query.Where(b => b.Category != "Hidden Gem");
            }

            if (showScams)
            {
                query = query.Where(b => b.IsSuspectedScam == true);
            }
            else
            {
                query = query.Where(b => b.IsSuspectedScam == false);

                if (hiddenGemsMode)
                {
                    query = query.Where(b => b.IsHiddenGem == true);
                }
            }

            IEnumerable<Business> sortedList = businessesList;

            if (string.IsNullOrEmpty(sortBy))
            {
                sortedList = sortedList.OrderBy(b => b.Name);
            }
            else
            {
                switch (sortBy!.ToLower())
                {
                    case "category":
                        sortedList = isAscending ? sortedList.OrderBy(b => b.Category) : sortedList.OrderByDescending(b => b.Category);
                        break;
                    case "categorytype":
                        sortedList = isAscending ? sortedList.OrderBy(b => b.CategoryType) : sortedList.OrderByDescending(b => b.CategoryType);
                        break;
                    case "country":
                        sortedList = isAscending ? sortedList.OrderBy(b => b.Country) : sortedList.OrderByDescending(b => b.Country);
                        break;
                    case "pricerange":
                        sortedList = isAscending
                            ? sortedList.OrderBy(b => b.PriceRange != null ? b.PriceRange.Length : 0)
                            : sortedList.OrderByDescending(b => b.PriceRange != null ? b.PriceRange.Length : 0);
                        break;
                    case "pricelevel":
                        sortedList = isAscending ? sortedList.OrderBy(b => b.PriceLevel) : sortedList.OrderByDescending(b => b.PriceLevel);
                        break;
                    case "averagerating":
                        sortedList = isAscending ? sortedList.OrderBy(b => b.AverageRating) : sortedList.OrderByDescending(b => b.AverageRating);
                        break;
                    case "moodtags":
                        sortedList = isAscending ? sortedList.OrderBy(b => b.MoodTags) : sortedList.OrderByDescending(b => b.MoodTags);
                        break;
                    case "city":
                        sortedList = isAscending ? sortedList.OrderBy(b => b.City) : sortedList.OrderByDescending(b => b.City);
                        break;
                    default:
                        sortedList = sortedList.OrderBy(b => b.Name);
                        break;
                }
            }
            return Ok(sortedList);
        }

        private int CalculateLevelFromBudget(decimal budget)
        {
            if (budget <= 10) return 1;
            if (budget <= 25) return 2;
            if (budget <= 50) return 3;
            return 4;
        }

        private int ConvertPriceRangeToLevel(string? priceRange)
        {
            if (string.IsNullOrEmpty(priceRange)) return 0;
            return priceRange.Length;
        }

        [HttpGet("top-rated")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Business>>> GetTopRated()
        {
            var businesses = await _context.Business
                                   .Where(b => b.Reviews.Any())
                                   .Select(b => new
                                   {
                                       b.BusinessId,
                                       b.Name,
                                       b.CategoryType,
                                       b.City,
                                       b.Address,
                                       b.PriceLevel,
                                       b.PriceRange,
                                       b.AverageRating,
                                       Rating = b.Reviews.Average(r => r.Rating) ,
                                       ReviewsCount = b.Reviews.Count
                                   })
                                   .OrderByDescending(x => x.Rating)
                                   .Take(3)
                                   .ToListAsync();
            var dtoList = businesses.Select(b => new TopBusinessDTO
            {
                BusinessId = b.BusinessId,
                Name = b.Name,
                CategoryType = b.CategoryType,
                City = b.City,
                Address = b.Address,
                PriceLevel = b.PriceLevel,
                AverageRating = b.AverageRating
            }).ToList();

            return Ok(dtoList);
        }

        [HttpGet("Stats")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<DashboardStatsDTO>> GetStats()
        {
            var userCount = await _context.Users.CountAsync();
            var businessCount = await _context.Business.CountAsync();
            var reviewsCount = await _context.Reviews.CountAsync();

            var recentUsers = await _context.Users
                .OrderByDescending(u => u.UserId)
                .Take(5)
                .Select(u => new RecentActivityDTO
                {
                    Name = u.FirstName + " " + u.LastName,
                    Role = "User",
                    Date = u.CreateAt,
                    Status = "Active"
                })
                .ToListAsync();

            var recentBusinessesForTable = await _context.Business
                .OrderByDescending(b => b.BusinessId)
                .Take(5)
                .Select(b => new RecentActivityDTO
                {
                    Name = b.Name,
                    Role = b.Category,
                    Date = b.CreateAt,
                    Status = b.IsSuspectedScam ? "Suspended" : "Active"
                })
                .ToListAsync();

            var combinedActivity = new List<RecentActivityDTO>();
            combinedActivity.AddRange(recentUsers);
            combinedActivity.AddRange(recentBusinessesForTable);
            var finalActivityList = combinedActivity.Take(6).ToList();

            var latestBusinessesForCards = await _context.Business
                .OrderByDescending(b => b.BusinessId)
                .Take(4)
                .Select(b => new BusinessSummaryDTO
                {
                    Id = b.BusinessId,
                    Name = b.Name ?? "",
                    Category = b.Category ?? "",
                    City = b.City ?? "",
                    Rating = b.AverageRating ?? 0,
                    ReviewCount = b.Reviews.Count,
                    PriceRange = b.PriceRange ?? "",
                    IsSuspectedScam = b.IsSuspectedScam,
                    isHiddenGem = b.IsHiddenGem,
                })
                .ToListAsync();

            var stats = new DashboardStatsDTO
            {
                TotalUsers = userCount,
                TotalBusinesses = businessCount,
                NewReviews = reviewsCount,
                RecentActivity = finalActivityList,
                LatestBusinesses = latestBusinessesForCards
            };

            return Ok(stats);
        }
    }
}