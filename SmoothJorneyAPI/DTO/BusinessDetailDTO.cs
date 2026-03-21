using SmoothJorneyAPI.Entities; 

namespace SmoothJorneyAPI.DTO
{
    public class BusinessDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty;
        public string MoodTags { get; set; } = string.Empty;
        public bool IsHiddenGem { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PriceRange { get; set; } = string.Empty;
        public int PriceLevel { get; set; }
        public double? AverageRating { get; set; } = 0;
        public bool IsSuspectedScam { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> GalleryPhotos { get; set; } = new List<string>();
    }
}