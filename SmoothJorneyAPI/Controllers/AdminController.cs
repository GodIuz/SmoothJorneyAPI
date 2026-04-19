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
    public class AdminController : ControllerBase
    {
        private readonly SmoothJorneyAPIContext _context;
        public AdminController(SmoothJorneyAPIContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Ο χρήστης δεν βρέθηκε.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { Message = $"Ο χρήστης {user.UserName} έχει διαγραφτεί." });
        }

        [HttpPut("users/{id}/promote")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PromoteToAdmin(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Ο χρήστης δεν βρέθηκε.");

            user.Role = "Admin";
            await _context.SaveChangesAsync();
            return Ok(new { Message = $"Ο {user.UserName} έγινε Admin!" });
        }

        [HttpDelete("businesses/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBusiness(int id)
        {
            var business = await _context.Business.FindAsync(id);
            if (business == null) return NotFound("Business not found");

            _context.Business.Remove(business);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Business deleted successfully." });
        }

        [HttpGet("businesses")]
        [Authorize(Roles = "Admin")]
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

        [HttpPut("update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBusiness(int id, [FromBody] UpdateBusinessDTO dto)
        {
            if (id != dto.BusinessId && dto.BusinessId != 0)
            {
                return BadRequest("Το ID στο URL δεν ταιριάζει με το ID του αντικειμένου.");
            }

            var business = await _context.Business.FindAsync(id);
            if (business == null) return NotFound("Η επιχείρηση δεν βρέθηκε.");

            business.Name = dto.Name;
            business.Description = dto.Description;
            business.Address = dto.Address;
            business.City = dto.City;
            business.Country = dto.Country;
            business.Phone = dto.Phone;
            business.Category = dto.Category;
            business.CategoryType = dto.CategoryType;
            business.PriceRange = dto.PriceRange;
            business.PriceLevel = dto.PriceLevel;
            business.MoodTags = dto.MoodTags;
            business.IsHiddenGem = dto.IsHiddenGem;
            business.IsSuspectedScam = dto.IsSuspectedScam;
            business.ImageUrl = dto.ImageUrl;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Η επιχείρηση ανανεώθηκε επιτυχώς!", Business = business });
        }

        [HttpPut("user/update:{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDTO dto)
        {
            if (id != dto.UserId) return BadRequest();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.City = dto.City;
            user.Country = dto.Country;
            user.Gender = dto.Gender;
            user.DateOfBirth = dto.DateOfBirth;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "User updated" });
        }

        
    }
}
