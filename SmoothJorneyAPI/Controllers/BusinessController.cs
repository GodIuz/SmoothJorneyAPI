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
    [Authorize]
    public class BusinessController : ControllerBase
    {
        private readonly SmoothJorneyAPIContext _context;

        public BusinessController(SmoothJorneyAPIContext context)
        {
            _context = context;
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

        [HttpGet]
        public async Task<IActionResult> GetBusinesses()
        {
            var businesses = await _context.Business
                .Select(b => new
                {
                    b.BusinessId,
                    b.Name,
                    b.Description,
                    b.City,
                    b.AverageRating,

                    CoverImageUrl = _context.BusinessImages
                        .Where(img => img.BusinessId == b.BusinessId && img.IsCover)
                        .Select(img => $"{Request.Scheme}://{Request.Host}/images/view/{img.Id}")
                        .FirstOrDefault()
                        ?? $"{Request.Scheme}://{Request.Host}/images/default-placeholder.svg"
                })
                .ToListAsync();

            return Ok(businesses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BusinessDetailDTO>> GetBusinessById(int id)
        {
            var business = await _context.Business.FindAsync(id);

            if (business == null) return NotFound("Η Επιχείρηση δεν βρέθηκε");

            var detailDto = new BusinessDetailDTO
            {
                Id = business.BusinessId,
                Name = business.Name,
                Category = business.Category,
                City = business.City,
                AverageRating = business.AverageRating,
                PriceLevel = business.PriceLevel,
                PriceRange = business.PriceRange,
                IsHiddenGem = business.IsHiddenGem,
                MoodTags = business.MoodTags,
                CoverImage = business.CoverImage,
                Address = business.Address,
                Phone = business.Phone ?? "N/A",
                Latitude = business.Latitude,
                Longitude = business.Longitude,
                IsSuspectedScam = business.IsSuspectedScam
            };

            return Ok(detailDto);
        }

        [HttpPost("create-business")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBusiness([FromBody] CreateBusinessDTO dto)
        {
            var business = new Business
            {
                Name = dto.Name,
                Category = dto.Category,
                CategoryType = dto.CategoryType,
                Country = dto.Country,
                City = dto.City,
                Address = dto.Address,
                Longitude = dto.Longtitude,
                Latitude = dto.Latitude,
                Phone = dto.Phone,
                AverageRating = dto.Rating,
                PriceLevel = dto.PriceLevel,
                PriceRange = dto.PriceRange,
                IsHiddenGem = dto.IsHiddenGem,
                IsSuspectedScam = dto.IsSuspectedScam,
                MoodTags = dto.MoodTags,
                CoverImage = new byte[0],
                CoverImageContentType = "image/png"
            };

            _context.Business.Add(business);
            await _context.SaveChangesAsync();
            return Ok(business);
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

        [HttpGet("image/{id}")]
        [Authorize]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> GetBusinessImage(int id)
        {
            var business = await _context.Business.FindAsync(id);

            if (business == null || business.CoverImage == null)
            {
                return NotFound();
            }
            return File(business.CoverImage, business.CoverImageContentType ?? "image/svg");
        }
    }
}
