using SmoooothJourneyApi.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmoothJorneyAPI.Entities
{
    public class Business
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BusinessId { get; set; }

        [Required, MaxLength(100)]
        public string? Name { get; set; }

        [Required, MaxLength(100)]
        public string? Category { get; set; }

        [Required, MinLength(100)]
        public string? CategoryType { get; set; }

        [Required, MaxLength(100)]

        public string? MoodTags { get; set; }

        [Required]
        public bool IsHiddenGem { get; set; }
        [Required]
        public bool IsSuspectedScam { get; set; }
        
        [Required,MaxLength(50)]
        public string? Address { get; set; }
        
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
        
        [Required,MaxLength(50)]
        public string? City { get; set; }

        [Required, MaxLength(50)]
        public string? Country { get; set; }

        [Required, MaxLength(30)]
        public string? Phone { get; set; }

        [Required, MaxLength(20)]
        public string? PriceRange { get; set; }

        [Required]
        public int PriceLevel { get; set; }

        [Required]
        public double? AverageRating { get; set; } = 0;

        [Required, MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public byte[]? CoverImage { get; set; }

        [Required]
        public string? CoverImageContentType { get; set; }

        public virtual ICollection<BusinessImage>? GalleryImages { get; set; }
        public virtual ICollection<Reviews>? Reviews { get; set; }
        public virtual ICollection<BusinessReport>? Reports { get; set; }
    }
}
