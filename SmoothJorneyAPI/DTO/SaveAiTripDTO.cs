using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace SmoothJorneyAPI.DTO
{
    public class SaveAiTripDTO
    {
        public int UserId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Mood { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public decimal TotalBudget { get; set; }

        public string? Description { get; set; }

        public List<AiDayDTO> Days { get; set; } = new List<AiDayDTO>();
    }
}
