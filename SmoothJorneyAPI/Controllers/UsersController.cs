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
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly SmoothJorneyAPIContext _context;
        public UsersController(SmoothJorneyAPIContext context)
        {
            _context = context;
        }
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDTO>> GetProfile()
        {
            var userId = GetUserIdFromToken();
            if (userId == 0) return Unauthorized();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");
            return new UserProfileDTO
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                City = user.City,
                Country = user.Country,
                Gender = user.Gender
            };
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDTO dto)
        {
            var userId = GetUserIdFromToken();
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            user.FirstName = dto.FirstName ?? user.FirstName;
            user.LastName = dto.LastName ?? user.LastName;
            user.Country = dto.Country;
            user.City = dto.City;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Profile updated successfully!" });
        }

        private int GetUserIdFromToken()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == "ID");
            if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }
    }
}
