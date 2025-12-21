namespace SmoothJorneyAPI.DTO
{
    public class SaveAiTripDTO
    {
        public string? City { get; set; }
        public decimal TotalBudget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<AiTripDayDTO>? PlanDays { get; set; }
    }
}
