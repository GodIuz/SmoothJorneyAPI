namespace SmoothJorneyAPI.DTO
{
    public class BusinessSummaryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int PriceLevel { get; set; }
        public string PriceRange { get; set; } = string.Empty;
        public bool IsSuspectedScam { get; set; } = false;
        public bool isHiddenGem { get; set; } = false;
    }
}
