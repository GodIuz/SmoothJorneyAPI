using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmoooothJourneyApi.Entities;
using SmoothJorneyAPI.Data;

namespace SmoooothJourneyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly SmoothJorneyAPIContext _context;

        public ImagesController(SmoothJorneyAPIContext context)
        {
            _context = context;
        }

        // 1. UPLOAD (Χειροκίνητο ανέβασμα)
        [HttpPost("upload/{businessId}")]
        public async Task<IActionResult> UploadImage(int businessId, IFormFile file)
        {
            var business = await _context.Business.FindAsync(businessId);
            if (business == null) 
            { 
                return NotFound("Η επιχείρηση δεν βρέθηκε"); 
            }

            if (file == null || file.Length == 0) 
            { 
                return BadRequest("Δεν έχει μεταφορτωθεί αρχείο!"); 
            }

            bool hasImages = await _context.BusinessImages.AnyAsync(i => i.BusinessId == businessId);

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var imageEntity = new BusinessImage
                {
                    BusinessId = businessId,
                    ImageData = memoryStream.ToArray(),
                    ContentType = file.ContentType,
                    IsCover = !hasImages,
                    UploadedAt = DateTime.UtcNow
                };

                _context.BusinessImages.Add(imageEntity);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Uploaded", IsCover = imageEntity.IsCover, Id = imageEntity.Id });
            }
        }

        [HttpGet("view/{id}")]
        public async Task<IActionResult> GetImage(int id)
        {
            var image = await _context.BusinessImages.FindAsync(id);
            if (image == null) return NotFound();
            return File(image.ImageData, image.ContentType);
        }

        [HttpGet("gallery/{businessId}")]
        public async Task<IActionResult> GetGallery(int businessId)
        {
            var images = await _context.BusinessImages
                .Where(i => i.BusinessId == businessId)
                .Select(i => new
                {
                    Id = i.Id,
                    IsCover = i.IsCover,
                    Url = $"{Request.Scheme}://{Request.Host}/images/view/{i.Id}"
                })
                .ToListAsync();

            return Ok(images);
        }
    }
}