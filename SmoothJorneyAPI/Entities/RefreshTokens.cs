using System.ComponentModel.DataAnnotations;

namespace SmoothJorneyAPI.Entities
{
    public class RefreshTokens
    {
        [Key]
        public int Id { get; set; }
        public string? Token { get; set; } // Η τυχαία συμβολοσειρά
        public string? JwtId { get; set; } // Το ID του Access Token που συνοδεύει
        public bool isUsed { get; set; }
        public bool isRevoked { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ExpiryDate { get; set; } // Πότε λήγει (π.χ. σε 6 μήνες)
        public bool Used { get; set; } // Αν έχει χρησιμοποιηθεί ήδη
        public bool Invalidated { get; set; } // Αν το ακυρώσαμε (π.χ. Logout)
        public int UserId { get; set; }
        public virtual Users? User { get; set; }
    }
}