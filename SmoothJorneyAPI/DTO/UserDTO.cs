namespace SmoothJorneyAPI.DTO
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Role { get; set; }
        public string? Gender { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }

    }
}
