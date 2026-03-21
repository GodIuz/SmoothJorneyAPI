namespace SmoothJorneyAPI.DTO
{
    public class BusinessSummaryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int PriceLevel { get; set; }
        public string PriceRange { get; set; } = string.Empty;
        public string MoodTags { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public bool IsSuspectedScam { get; set; } = false;
        public bool isHiddenGem { get; set; } = false;
        public double ReviewCount { get; set; } = 0;
        public string Longtitude { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public byte[]? CoverImage { get; set; }
        public string? Image { get; set; }
    }
}
