using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace SmoothJorneyAPI.Services
{
    public class Argon2PasswordHasher
    {
        private const int DegreeOfParallelism = 8;
        private const int MemorySize = 65536;
        private const int Iterations = 4;

        public (string Hash, string Salt) HashPassword(string password)
        {
            var saltBytes = CreateSalt();
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
            argon2.Salt = saltBytes;
            argon2.DegreeOfParallelism = DegreeOfParallelism;
            argon2.MemorySize = MemorySize;
            argon2.Iterations = Iterations;
            var hashBytes = argon2.GetBytes(16);
            return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
        }

        public bool VerifyPassword(string storedHash, string storedSalt, string providedPassword)
        {
            try
            {
                var saltBytes = Convert.FromBase64String(storedSalt);
                var storedHashBytes = Convert.FromBase64String(storedHash);
                using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(providedPassword));
                argon2.Salt = saltBytes;
                argon2.DegreeOfParallelism = DegreeOfParallelism;
                argon2.MemorySize = MemorySize;
                argon2.Iterations = Iterations;
                var newHashBytes = argon2.GetBytes(16);
                return newHashBytes.SequenceEqual(storedHashBytes);
            }
            catch
            {
                return false;
            }
        }
        
        private byte[] CreateSalt()
        {
            var buffer = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(buffer);
            return buffer;
        }
    }
}