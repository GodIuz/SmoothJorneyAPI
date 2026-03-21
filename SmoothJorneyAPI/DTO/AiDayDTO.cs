namespace SmoothJorneyAPI.DTO
{
    public class AiDayDTO
    {
        public int Day { get; set; }
        public List<AiActivityDTO> Activities { get; set; } = new List<AiActivityDTO>();
    }
}