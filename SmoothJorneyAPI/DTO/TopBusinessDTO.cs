namespace SmoothJorneyAPI.DTO
{
    public class TopBusinessDTO
    {
        public int BusinessId { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? CategoryType { get; set; }
        public string? Address { get; set; }
        public double? AverageRating { get; set; }
        public int PriceLevel { get; set; }
        public string? CoverImage { get; set; }
    }
}
