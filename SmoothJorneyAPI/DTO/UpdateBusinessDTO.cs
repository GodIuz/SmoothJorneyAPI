using System.ComponentModel.DataAnnotations;

namespace SmoothJorneyAPI.DTO
{
    public class UpdateBusinessDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string CategoryType { get; set; } = string.Empty;

        public string PriceRange { get; set; } = string.Empty;

        public string MoodTags { get; set; } = string.Empty;

        public IFormFile? ImageFile { get; set; }
    }
}
