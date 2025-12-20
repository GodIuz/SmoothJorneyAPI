using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmoothJorneyAPI.Entities
{
    public class TripCollaborator
    {
        [Key]
        public int Id { get; set; }

        public int TripId { get; set; }
        
        [ForeignKey("TripId")]
        public virtual Trips? Trip { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }
    }
}