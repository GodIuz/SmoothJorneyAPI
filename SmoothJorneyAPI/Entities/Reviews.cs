using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmoothJorneyAPI.Entities
{
    public class Reviews
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string? Content { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public double AverageRating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? Sentiment { get; set; }

        public int BusinessId { get; set; }

        [ForeignKey("BusinessId")]
        public virtual Business? Business{ get; set; }

        public int? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }
    }
}