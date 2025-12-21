using SmoothJorneyAPI.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmoooothJourneyApi.Entities
{
    public class BusinessImage
    {
        [Key]
        public int Id { get; set; }

        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business? Business { get; set; }

        public byte[]? ImageData { get; set; }
        
        public string? ContentType { get; set; } 

        public bool IsCover { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}