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
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PriceRange { get; set; } = string.Empty;
        public int PriceLevel { get; set; }
        public double? AverageRating { get; set; } = 0;
        public double Rating { get; set; }
        public byte[]? CoverImage { get; set; }
        public bool IsSuspectedScam { get; set; }
    }
}
