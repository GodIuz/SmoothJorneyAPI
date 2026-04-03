namespace SmoothJorneyAPI.DTO
{
    public class RegisterDTO
    {
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get;set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Gender { get; set; }
        public DateOnly DateOFBirth { get; set; }
    }
}
