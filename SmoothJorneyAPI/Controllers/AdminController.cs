using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmoothJorneyAPI.Data;
using SmoothJorneyAPI.DTO;

namespace SmoothJorneyAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly SmoothJorneyAPIContext _context;
        public AdminController(SmoothJorneyAPIContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    u.Email,
                    u.Role,
                    u.FirstName,
                    u.LastName,
                    u.Country,
                    u.City,
                    u.DateOfBirth,
                    u.Gender,
                    RegisteredOn = u.CreateAt
                })
                .ToListAsync();
            return Ok(users);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Ο χρήστης δεν βρέθηκε.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { Message = $"Ο χρήστης {user.UserName} έχει διαγραφτεί." });
        }

        [HttpPut("users/{id}/promote")]
        public async Task<IActionResult> PromoteToAdmin(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Ο χρήστης δεν βρέθηκε.");

            user.Role = "Admin";
            await _context.SaveChangesAsync();
            return Ok(new { Message = $"Ο {user.UserName} έγινε Admin!" });
        }

        [HttpDelete("businesses/{id}")]
        public async Task<IActionResult> DeleteBusiness(int id)
        {
            var business = await _context.Business.FindAsync(id);
            if (business == null) return NotFound("Business not found");

            _context.Business.Remove(business);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Business deleted successfully." });
        }

        [HttpPut("businesses/{id}/toggle-gem")]
        public async Task<IActionResult> ToggleHiddenGem(int id)
        {
            var business = await _context.Business.FindAsync(id);
            if (business == null) return NotFound("Η επιχείρηση δεν βρέθηκε.");

            business.IsHiddenGem = !business.IsHiddenGem;

            if (business.IsHiddenGem)
            {
                business.IsSuspectedScam = false;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = business.IsHiddenGem ? "Επιχείρηση επισημασμένη ως Hidden Gem!" : "Η επιχείρηση δεν είναι πλέον Hidden Gem.",
                IsHiddenGem = business.IsHiddenGem
            });
        }

        [HttpPut("businesses/{id}/toggle-scam")]
        public async Task<IActionResult> ToggleScam(int id)
        {
            var business = await _context.Business.FindAsync(id);
            if (business == null) return NotFound("Η επιχείρηση δεν βρέθηκε");

            business.IsSuspectedScam = !business.IsSuspectedScam;

            if (business.IsSuspectedScam)
            {
                business.IsHiddenGem = false;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = business.IsSuspectedScam ? "h eπιχείρηση έχει χαρακτηριστεί ως ΑΠΑΤΗ!" : "Η επιχείρηση έχει επισημανθεί ως ασφαλής.",
                IsSuspectedScam = business.IsSuspectedScam
            });
        }

        [HttpGet("businesses")]
        public async Task<IActionResult> GetAllBusinessesForAdmin()
        {
            var businesses = await _context.Business
                .Select(b => new
                {
                    b.BusinessId,
                    b.Name,
                    b.City,
                    b.AverageRating,
                    b.IsHiddenGem,
                    b.IsSuspectedScam
                })
                .ToListAsync();

            return Ok(businesses);
        }

        [HttpPut("businesses/{id}/toggle-scam-status")]
        public async Task<IActionResult> ToggleScamStatus(int id)
        {
            var business = await _context.Business.FindAsync(id);
            if (business == null) return NotFound();

            business.IsSuspectedScam = !business.IsSuspectedScam;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Η κατάσταση της επιχειρηματικής απάτης άλλαξε σε: {business.IsSuspectedScam}" });
        }

        [HttpGet("reviews")]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Business)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    User = r.User.UserName,
                    Business = r.Business.Name,
                    r.Rating,
                    r.Content,
                    r.CreatedAt
                })
                .ToListAsync();
            return Ok(reviews);
        }

        [HttpDelete("reviews/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Η αξιολόγηση σβήστηκε" });
        }

        [HttpPut("businesses/{id}")]
        public async Task<IActionResult> UpdateBusiness(int id, [FromBody] UpdateBusinessDTO dto)
        {
            var business = await _context.Business.FindAsync(id);
            if (business == null) return NotFound("Η επιχείρηση δεν βρέθηκε.");

            business.Name = dto.Name;
            business.City = dto.City;
            business.Description = dto.Description;
            business.PriceRange = dto.PriceRange;
            business.Category = dto.Category;
            business.CategoryType = dto.CategoryType;
            business.MoodTags = dto.MoodTags;
            if (dto.ImageFile != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await dto.ImageFile.CopyToAsync(memoryStream);
                    business.CoverImage = memoryStream.ToArray();
                    business.CoverImageContentType = dto.ImageFile.ContentType;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Η επιχείρηση ανανεώθηκε ", Business = business });
        }

        [HttpGet("top-businesses")]
        public async Task<IActionResult> GetTopBusinesses()
        {
            var topList = await _context.Business
                .OrderByDescending(b => b.AverageRating)
                .Take(5)
                .Select(b => new
                {
                    b.Name,
                    b.City,
                    Rating = b.AverageRating,
                    ReviewsCount = _context.Reviews.Count(r => r.BusinessId == b.BusinessId)
                })
                .ToListAsync();

            return Ok(topList);
        }

        [HttpGet("image/{id}")]
        public async Task<IActionResult> GetBusinessImage(int id)
        {
            var business = await _context.Business.FindAsync(id);

            if (business == null || business.CoverImage == null)
            {
                return NotFound();
            }

            return File(business.CoverImage, business.CoverImageContentType ?? "image/jpeg");
        }
    }
}
