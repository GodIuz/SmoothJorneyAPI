using System.ComponentModel.DataAnnotations;

namespace SmoothJorneyAPI.DTO
{
    public class CreateReviewsDTO
    {
        public int BusinessId { get; set; }

        public string Content { get; set; } = string.Empty;

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }
    }
}
