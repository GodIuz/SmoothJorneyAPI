namespace SmoothJorneyAPI.DTO
{
    public class CreateManualTripDTO
    {
        public string? Title { get; set; }
        public string? City { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalBudget { get; set; }
    }
}
