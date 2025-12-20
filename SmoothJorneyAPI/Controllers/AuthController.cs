using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmoothJorneyAPI.Data;
using SmoothJorneyAPI.DTO;
using SmoothJorneyAPI.Entities;
using SmoothJorneyAPI.Interfaces;
using SmoothJorneyAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmoothJorneyAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SmoothJorneyAPIContext _context;
        private readonly Argon2PasswordHasher _hasher;
        private readonly IEmailService _emailService;

        public AuthController(IConfiguration configuration, SmoothJorneyAPIContext context, Argon2PasswordHasher hasher, IEmailService emailService)
        {
            _configuration = configuration;
            _context = context;
            _hasher = hasher;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("User already exists.");
            }

            var securityData = _hasher.HashPassword(dto.Password);
            var user = new Users
            {
                UserName = dto.Username,
                Email = dto.Email,
                EmailConfirmed = false,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PasswordHash = securityData.Hash,
                PasswordSalt = securityData.Salt,
                Country = dto.Country,
                City = dto.City,
                DateOfBirth = dto.DateOFBirth,
                Gender = dto.Gender,
                Role = dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully!" });
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null) return Unauthorized("Invalid Email or Password");
            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
            {
                var timeLeft = user.LockoutEnd - DateTime.UtcNow;
                return Unauthorized($"Account is locked. Try again in {timeLeft.Value.Minutes} minutes and {timeLeft.Value.Seconds} seconds.");
            }
            var isCorrect = _hasher.VerifyPassword(user.PasswordHash, user.PasswordSalt, dto.Password);

            if (!isCorrect)
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 3)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(5);
                    user.FailedLoginAttempts = 0;

                    await _context.SaveChangesAsync();
                    return Unauthorized("Account locked due to too many failed attempts. Try again in 5 minutes.");
                }

                await _context.SaveChangesAsync();

                int attemptsLeft = 3 - user.FailedLoginAttempts;
                return Unauthorized($"Invalid Password. You have {attemptsLeft} attempts left before lockout.");
            }

            if (user.FailedLoginAttempts > 0 || user.LockoutEnd != null)
            {
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                await _context.SaveChangesAsync();
            }

            var authResult = GenerateJwtToken(user);
            var refreshToken = new RefreshTokens
            {
                JwtId = authResult.Id,
                isUsed = false,
                isRevoked = false,
                UserId = user.UserId,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                Token = CreateRandomToken() + Guid.NewGuid()
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            return new UserDTO
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Country = user.Country,
                City = user.City,
                Role = user.Role,
                Gender = user.Gender,
                DateOfBirth = (DateOnly)user.DateOfBirth,
                Token = authResult.Token,
                RefreshToken = refreshToken.Token
            };
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Ok(new { Message = "If user exists, reset instructions have been sent to your email." });

            user.PasswordResetToken = CreateRandomToken();
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();
            var resetLink = $"https://localhost:7003/Auth/reset-password?token={user.PasswordResetToken}";

            var emailSubject = "Επαναφορά Κωδικού - Smooth Journey";
            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2 style='color: #2c3e50;'>Αίτημα Αλλαγής Κωδικού</h2>
                    <p>Γεια σου {user.FirstName},</p>
                    <p>Λάβαμε ένα αίτημα για επαναφορά του κωδικού πρόσβασης στον λογαριασμό σου.</p>
                    <p>Πάτα το παρακάτω κουμπί για να ορίσεις νέο κωδικό:</p>
                    <br>
                    <a href='{resetLink}' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Αλλαγή Κωδικού</a>
                    <br><br>
                    <p style='color: #7f8c8d; font-size: 12px;'>Αν δεν ζήτησες εσύ την αλλαγή, απλά αγνόησε αυτό το email. Ο κωδικός σου είναι ασφαλής.</p>
                </div>";

            try
            {
                await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);

                return Ok(new { Message = "Password reset link has been sent to your email." });
            }
            catch (Exception ex)
            {
                return BadRequest("Could not send email. Please try again later. Error: " + ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDTO tokenRequest)
        {
            var storedRefreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);

            if (storedRefreshToken == null)
            {
                return BadRequest(new { Message = "Refresh token does not exist" });
            }

            if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
            {
                return BadRequest(new { Message = "Refresh token has expired, please login again" });
            }

            if (storedRefreshToken.isRevoked || storedRefreshToken.isUsed)
            {
                return BadRequest(new { Message = "Invalid refresh token" });
            }

            storedRefreshToken.isUsed = true;
            _context.RefreshTokens.Update(storedRefreshToken);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(storedRefreshToken.UserId);
            if (user == null) return BadRequest(new { Message = "User not found" });
            var authResult = GenerateJwtToken(user);

            var newRefreshToken = new RefreshTokens
            {
                JwtId = authResult.Id,
                isUsed = false,
                isRevoked = false,
                UserId = user.UserId,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                Token = CreateRandomToken() + Guid.NewGuid()
            };

            await _context.RefreshTokens.AddAsync(newRefreshToken);
            await _context.SaveChangesAsync();

            return Ok(new UserDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = authResult.Token,
                RefreshToken = newRefreshToken.Token
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == dto.Token);
            if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired token.");
            }

            var securityData = _hasher.HashPassword(dto.NewPassword);
            user.PasswordHash = securityData.Hash;
            user.PasswordSalt = securityData.Salt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Password changed successfully!" });
        }

        [HttpPost("send-verification-email")]
        [Authorize]
        public async Task<IActionResult> SendVerificationEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) return BadRequest("User not found");
            if (user.EmailConfirmed) return BadRequest("Email is already verified.");

            var verifyToken = CreateRandomToken();
            user.VerificationToken = verifyToken;
            await _context.SaveChangesAsync();

            var verifyLink = $"https://localhost:7003/Auth/verify-email?token={verifyToken}";

            var body = $@"
                <div style='background-color: #f4f4f4; padding: 20px; font-family: Arial;'>
                    <div style='max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px;'>
                        <h2 style='color: #27ae60; text-align: center;'>Smooth Journey</h2>
                        <p>Γεια σου {user.FirstName},</p>
                        <p>Ευχαριστούμε για την εγγραφή! Για να ενεργοποιήσεις τον λογαριασμό σου, πάτα το παρακάτω κουμπί:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{verifyLink}' style='background-color: #27ae60; color: white; padding: 12px 24px; text-decoration: none; font-weight: bold; border-radius: 5px;'>ΕΠΙΒΕΒΑΙΩΣΗ EMAIL</a>
                        </div>
                    </div>
                </div>";

            try
            {
                await _emailService.SendEmailAsync(user.Email, "Επιβεβαίωση Email", body);
                return Ok(new { Message = "Verification email sent! Check your inbox." });
            }
            catch (Exception ex)
            {
                return BadRequest("Error sending email: " + ex.Message);
            }
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            // Ψάχνουμε αν υπάρχει χρήστης με αυτό το token
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

            if (user == null)
            {
                return BadRequest("Invalid or expired token.");
            }

            // Ενεργοποίηση Χρήστη
            user.VerifiedAt = DateTime.UtcNow;
            user.EmailConfirmed = true;
            user.VerificationToken = null;

            await _context.SaveChangesAsync();

            return Ok("Email confirmed successfully! You can now close this tab and login.");
        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        private (string Token, string Id) GenerateJwtToken(Users user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]);

            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("ID", user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                 }),

                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return (jwtToken, token.Id);
        }
    }
}