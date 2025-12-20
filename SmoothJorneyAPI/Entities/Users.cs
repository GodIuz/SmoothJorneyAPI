using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmoothJorneyAPI.Entities
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required,MaxLength(50)]
        public string? UserName { get; set; }

        [Required,MaxLength(50)]
        public string? FirstName { get; set; }

        [Required, MaxLength(50)]
        public string? LastName { get; set; }

        [Required,MaxLength(100)]
        public string? Country { get; set; }

        [Required,MaxLength(100)]
        public string? City { get; set; }

        [Required,EmailAddress]
        public string? Email { get; set; }

        [Required,MinLength(8)]
        public string? PasswordHash { get; set; }

        [Required]
        public string? PasswordSalt { get; set; }

        [Required]
        public DateOnly? DateOfBirth { get; set; }

        [Required]
        public string? Gender { get; set;}

        [Required]
        public string? Role { get; set; } = "User";
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public bool EmailConfirmed { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordExpiry { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }
        public string? VerificationToken { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public virtual ICollection<Trips>? Trips { get; set; }
        public virtual ICollection<Reviews>? Reviews { get; set; }
    }
}
