using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmoothJorneyAPI.Entities
{
    public class Favorite
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }

        public int BusinessId { get; set; }

        [ForeignKey("BusinessId")]
        public virtual Business? Business { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
