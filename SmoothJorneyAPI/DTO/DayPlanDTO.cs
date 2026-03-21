namespace SmoothJorneyAPI.DTO
{
    public class DayPlanDTO
    {
        public int Day { get; set; }
        public List<ActivityPlanDto> Activities { get; set; } = new();
    }
}