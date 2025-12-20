namespace SmoothJorneyAPI.DTO
{
    public class MoodTripRequestDTO
    {
        public string Mood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal Budget { get; set; }
    }
}