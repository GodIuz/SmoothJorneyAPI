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
    public class ReviewsController : ControllerBase
    {
        private readonly SmoothJorneyAPIContext _context;
        public ReviewsController(SmoothJorneyAPIContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Reviews>> GetReviewsById(int id)
        {
            var reviews = await _context.Reviews.FindAsync(id);

            if (reviews == null)
            {
                return NotFound();
            }

            return reviews;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutReviews(int id, Reviews reviews)
        {
            if (id != reviews.Id)
            {
                return BadRequest();
            }

            _context.Entry(reviews).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Reviews>> PostReviews(Reviews reviews)
        {
            _context.Reviews.Add(reviews);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReviews", new { id = reviews.Id }, reviews);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            int businessId = review.BusinessId;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            await UpdateBusinessRating(businessId);

            return Ok(new { Message = "Review deleted and average rating updated." });
        }

        private bool ReviewsExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewsDTO dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ID" || c.Type == "Id");

            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            var userId = int.Parse(userIdClaim.Value);
            var businessExists = await _context.Business.AnyAsync(b => b.BusinessId == dto.BusinessId);

            if (!businessExists)
                return NotFound("Business not found.");

            var review = new Reviews
            {
                UserId = userId,
                BusinessId = dto.BusinessId,
                Content = dto.Content,
                Rating = dto.Rating,
                CreatedAt = DateTime.UtcNow,
                Sentiment = "Pending"
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
                    Content = r.Content,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt,
                    Sentiment = r.Sentiment
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
                business.AverageRating = await businessReviews.AverageAsync(r => r.Rating);
            }
            else
            {
                business.AverageRating = 0;
            }

            await _context.SaveChangesAsync();
        }
    }
}
