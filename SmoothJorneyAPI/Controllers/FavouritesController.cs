using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmoothJorneyAPI.Data;
using SmoothJorneyAPI.Entities;
using System.Security.Claims;

namespace SmoothJorneyAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    //[Authorize(Roles = "User")]
    public class FavouritesController : ControllerBase
    {
        private readonly SmoothJorneyAPIContext _context;

        public FavouritesController(SmoothJorneyAPIContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetFavorites()
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized("User not found");

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Business)
                .ThenInclude(b => b.Reviews)
                .Select(f => f.Business)
                .ToListAsync();

            return Ok(favorites);
        }

        [HttpPost("toggle/{businessId}")]
        public async Task<IActionResult> ToggleFavorite(int businessId)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var existingFav = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BusinessId == businessId);

            if (existingFav != null)
            {
                _context.Favorites.Remove(existingFav);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Removed", isFavorite = false });
            }
            else
            {
                var newFav = new Favorite
                {
                    UserId = userId,
                    BusinessId = businessId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Favorites.Add(newFav);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Added", isFavorite = true });
            }
        }

        [HttpGet("ids")]
        public async Task<ActionResult<IEnumerable<int>>> GetFavoriteIds()
        {
            var userId = GetUserId();
            if (userId == 0) return new List<int>();

            return await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.BusinessId)
                .ToListAsync();
        }

        private int GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "Id" || c.Type == "ID" || c.Type == ClaimTypes.NameIdentifier);
            if (claim == null) return 0;
            return int.Parse(claim.Value);
        }
    }
}