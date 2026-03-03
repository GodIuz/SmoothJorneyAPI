

namespace SmoothJorneyAPI.DTO
{
    public class CreateBusinessDTO
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string CategoryType { get; set; }
        public string MoodTags { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string PriceRange { get; set; }
        public int PriceLevel { get; set; }
        public string Description { get; set; }
        public bool IsHiddenGem { get; set; }
        public bool IsSuspectedScam { get; set; }
        public string? ImageUrl { get; set; }
    }
}