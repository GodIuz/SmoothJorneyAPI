namespace SmoothJorneyAPI.DTO
{
    public class UserProfileDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public DateTime? CreateOn { get; set; }
    }
}
