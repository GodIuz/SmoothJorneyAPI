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

        public string? City { get; set; }

        [MaxLength(50)]
        public string? Mood { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBudget { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }

        public virtual ICollection<TripItem>? TripItems { get; set; }

        public int NumberOfPeople { get; set; } = 1;
    }
}