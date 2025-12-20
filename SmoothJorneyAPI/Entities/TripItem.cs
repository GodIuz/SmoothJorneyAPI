using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmoothJorneyAPI.Entities
{
    public class TripItem
    {
        [Key]
        public int TripItemId { get; set; }

        public int TripId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedCost { get; set; }

        [ForeignKey("TripId")]
        public virtual Trips? Trip { get; set; }

        public int BusinessId { get; set; }

        [ForeignKey("BusinessId")]
        public virtual Business? Business { get; set; }

        public DateTime ScheduledTime { get; set; }
    }
}