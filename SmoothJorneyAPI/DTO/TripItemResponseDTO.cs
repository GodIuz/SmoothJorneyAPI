namespace SmoothJorneyAPI.DTO
{
    public class TripItemResponseDTO
    {
        public string BusinessName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public decimal Cost { get; set; }
    }
}