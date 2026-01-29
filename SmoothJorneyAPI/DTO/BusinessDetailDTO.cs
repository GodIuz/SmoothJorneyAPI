using SmoothJorneyAPI.Entities; // Βεβαιώσου ότι έχεις αυτό το using αν χρειαστεί, αλλιώς σβήσ' το

namespace SmoothJorneyAPI.DTO
{
    public class BusinessDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty; // Αν το χρησιμοποιείς
        public string MoodTags { get; set; } = string.Empty;
        public bool IsHiddenGem { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PriceRange { get; set; } = string.Empty;

        // Προσοχή εδώ: Αν στη βάση το έχεις string ή int, προσάρμοσέ το ανάλογα. 
        // Στον Controller το έβλεπα ως string σε κάποια σημεία. Αν είναι int κράτα το int.
        public int PriceLevel { get; set; }

        public double? AverageRating { get; set; } = 0;
        public bool IsSuspectedScam { get; set; }

        // ✅ 1. COVER PHOTO (Αντικαθιστά το ImageUrl για σαφήνεια)
        public string? CoverPhoto { get; set; }

        // ✅ 2. GALLERY PHOTOS (Λίστα με strings, όχι Entities)
        // Αυτό διορθώνει το error 'does not contain definition for GalleryPhotos'
        public List<string> GalleryPhotos { get; set; } = new List<string>();
    }
}