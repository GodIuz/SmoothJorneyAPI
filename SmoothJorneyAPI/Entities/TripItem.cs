using SmoothJorneyAPI;
using SmoothJorneyAPI.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class TripItem
{
    [Key]
    public int TripItemId { get; set; }

    public int TripId { get; set; }

    [Required]
    public string? Title { get; set; }

    [Required]
    public string? Description { get; set; }

    [ForeignKey("TripId")]
    public virtual Trips? Trip { get; set; }

    public int? BusinessId { get; set; }

    [ForeignKey("BusinessId")]
    public virtual Business? Business { get; set; }

    public DateTime ScheduledTime { get; set; }
    public bool IsVisited { get; set; } = false;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Cost { get; set; }

    public string? Duration { get; set; }
}