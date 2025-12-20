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
        public bool IsHiddenGem { get; set; }
        public string MoodTags { get; set; } = string.Empty; 
        public byte[]? CoverImage { get; set; }
    }
}
