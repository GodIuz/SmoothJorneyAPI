namespace SmoothJorneyAPI.DTO
{
    public class MoodTripRequestDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Days { get; set; }
        public string Mood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public int NumberOfPeople { get; set; }
    }
}