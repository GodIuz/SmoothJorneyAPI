namespace SmoothJorneyAPI.DTO
{
    public class CreateTripDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal TotalBudget { get; set; }
    }
}
