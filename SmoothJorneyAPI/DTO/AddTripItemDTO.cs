namespace SmoothJorneyAPI.DTO
{
    public class AddTripItemDTO
    {
        public int TripId { get; set; }
        public int BusinessId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public decimal EstimatedCost { get; set; }
    }
}
