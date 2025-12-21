namespace SmoothJorneyAPI.DTO
{
    public class AddTripItemDTO
    {
        public int TripId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime ScheduledTime { get; set; }
        public decimal Cost { get; set; }
        public int BusinessId { get; set; }
        public decimal EstimatedCost { get; set; }
    }
}
