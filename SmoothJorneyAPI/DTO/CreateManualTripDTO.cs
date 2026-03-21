using System.ComponentModel.DataAnnotations;

namespace SmoothJorneyAPI.DTO
{
    public class CreateManualTripDTO
    {
        public string? Title { get; set; }
        public string? City { get; set; }
        [MaxLength(50)]
        public string? Mood { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalBudget { get; set; }
    }
}
