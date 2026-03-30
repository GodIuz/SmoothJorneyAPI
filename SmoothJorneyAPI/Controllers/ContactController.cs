using Ganss.Xss;
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
    public class ContactController : ControllerBase
    {
        private readonly SmoothJorneyAPIContext _context;
        private readonly IHtmlSanitizer _sanitizer;

        public ContactController(SmoothJorneyAPIContext context, IHtmlSanitizer sanitizer)
        {
            _context = context;
            _sanitizer = sanitizer;
        }

        [HttpPost("send")]
        [AllowAnonymous]
        public async Task<IActionResult> SendMessage([FromBody] ContactMessageDTO dto)
        {
            var message = new ContactMessage
            {
                FirstName = _sanitizer.Sanitize(dto.FirstName),
                LastName = _sanitizer.Sanitize(dto.LastName),
                Email = _sanitizer.Sanitize(dto.Email),
                Subject = _sanitizer.Sanitize(dto.Subject),
                MessageBody = _sanitizer.Sanitize(dto.MessageBody),
                CreatedAt = DateTime.UtcNow
            };

            _context.ContactMessages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Το μήνυμά σας εστάλη με ασφάλεια!" });
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllMessages()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
            return Ok(messages);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null) return NotFound();

            _context.ContactMessages.Remove(message);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
