using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmoothJorneyAPI.Data;
using SmoothJorneyAPI.DTO;
using SmoothJorneyAPI.Entities;
using System.Security.Claims;

namespace SmoothJorneyAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly SmoothJorneyAPIContext _context;
        public ReviewsController(SmoothJorneyAPIContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Reviews>> GetReviewsById(int id)
        {
            var reviews = await _context.Reviews.FindAsync(id);

            if (reviews == null)
            {
                return NotFound();
            }

            return reviews;
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        private static bool DetectMaliciousActivity(Reviews r)
        {
            if (string.IsNullOrEmpty(r.Content)) return false;

            var contentLower = r.Content.ToLower();
            var blacklist = new List<string> { "spam", "scam", "fake", "badword1", "script", "alert(", "onclick" };
            bool hasBadWords = blacklist.Any(word => contentLower.Contains(word));
            var promptInjectionPatterns = new List<string>
    {
        "ignore previous instructions",
        "ignore all instructions",
        "system prompt",
        "αγνόησε τις οδηγίες",
        "reveal your secrets"
    };
            bool isPromptInjection = promptInjectionPatterns.Any(p => contentLower.Contains(p));

            bool isReviewBombing = r.Rating <= 1 && r.Content.Length < 10;

            return hasBadWords || isReviewBombing || isPromptInjection;
        }


        [HttpPost("add")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewsDTO dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ID" || c.Type == "Id");
            if (userIdClaim == null) return Unauthorized("User ID not found in token.");

            var userId = int.Parse(userIdClaim.Value);
            var businessExists = await _context.Business.AnyAsync(b => b.BusinessId == dto.BusinessId);

            if (!businessExists) return NotFound("Business not found.");
            var sanitizer = new HtmlSanitizer();
            var cleanContent = sanitizer.Sanitize(dto.Content ?? "");

            var review = new Reviews
            {
                UserId = userId,
                BusinessId = dto.BusinessId,
                Content = cleanContent,
                Rating = dto.Rating,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            await UpdateBusinessRating(dto.BusinessId);

            return Ok(new { Message = "Review added successfully!" });
        }

        [HttpGet("business/{businessId}")]
        public async Task<ActionResult<IEnumerable<ReviewResponseDTO>>> GetReviews(int businessId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.BusinessId == businessId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewResponseDTO
                {
                    Id = r.Id,
                    UserName = r.User != null ? r.User.FirstName + " " + r.User.LastName : "Unknown User",
                    Content = r.Content ?? "",
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        private async Task UpdateBusinessRating(int businessId)
        {
            var business = await _context.Business.FindAsync(businessId);
            if (business == null) return;

            var businessReviews = _context.Reviews.Where(r => r.BusinessId == businessId);

            if (await businessReviews.AnyAsync())
            {
                business.AverageRating = (decimal)await businessReviews.AverageAsync(r => r.Rating);
            }
            else
            {
                business.AverageRating = 0;
            }

            await _context.SaveChangesAsync();
        }

        [HttpGet("my-reviews")]
        [Authorize]
        public async Task<IActionResult> GetMyReviews()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ID" || c.Type == "Id" || c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Δεν βρέθηκε ID χρήστη στο Token." });
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return BadRequest(new { message = "Μη έγκυρο User ID." });
            }

            var reviews = await _context.Reviews
                .Where(r => r.UserId == userId)
                .Include(r => r.Business)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    id = r.Id,
                    businessId = r.BusinessId,
                    businessName = r.Business != null ? r.Business.Name : "Άγνωστη Επιχείρηση",
                    rating = r.Rating,
                    comment = r.Content,
                    createdAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all-reviews")]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Business)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new {
                    r.Id,
                    BusinessName = r.Business.Name,
                    r.User.UserName,
                    r.Rating,
                    r.Content,
                    r.CreatedAt,
                    IsSuspicious = DetectMaliciousActivity(r)
                })
                .ToListAsync();

            return Ok(reviews);
        }


        [HttpDelete("delete-review/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Η κριτική διαγράφηκε επιτυχώς." });
        }
    }
}
