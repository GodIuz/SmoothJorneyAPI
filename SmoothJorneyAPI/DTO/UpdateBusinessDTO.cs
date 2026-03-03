using System.ComponentModel.DataAnnotations;

namespace SmoothJorneyAPI.DTO
{
    public class UpdateBusinessDTO
    {
        public int BusinessId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string CategoryType { get; set; } = string.Empty;

        public string PriceRange { get; set; } = string.Empty;

        public int PriceLevel { get; set; }

        public string MoodTags { get; set; } = string.Empty;

        public bool IsHiddenGem { get; set; }

        public bool IsSuspectedScam { get; set; }

        public string? ImageUrl { get; set; }
    }
}