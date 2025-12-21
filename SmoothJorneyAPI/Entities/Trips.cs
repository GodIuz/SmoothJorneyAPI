using SmoothJorneyAPI.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmoothJorneyAPI
{
    public class Trips
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TripId { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBudget { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentCost { get; set; } = 0;

        public string ShareToken { get; set; } = Guid.NewGuid().ToString();

        public double AverageRating { get; set; } = 0;

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }

        public virtual ICollection<TripItem>? TripItems { get; set; }

        public virtual ICollection<TripCollaborator>? Collaborators { get; set; }
    }
}