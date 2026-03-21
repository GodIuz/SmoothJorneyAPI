namespace SmoothJorneyAPI.DTO
{
    public class ReviewResponseDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Sentiment { get; set; }
    }
}
