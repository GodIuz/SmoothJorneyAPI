namespace SmoothJorneyAPI.DTO
{
    public class ActivityPlanDto
    {
        public string Time { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public int? BusinessId { get; set; }
    }
}