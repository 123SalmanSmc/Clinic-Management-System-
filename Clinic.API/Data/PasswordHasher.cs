using System.Security.Cryptography;
using BCrypt.Net;

namespace Clinic.API.Data
{
    public sealed class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 100000;

        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

        public string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySize);

            return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
        }

        public bool Verify(string password, string hashWithSalt)
        {
            // 1. Split the stored hash string to get the hash and the salt
            // Support both PBKDF2(hexHash-hexSalt) and bcrypt ($2a$...) stored hashes
            if (string.IsNullOrWhiteSpace(hashWithSalt)) return false;

            if (hashWithSalt.StartsWith("$2a$") || hashWithSalt.StartsWith("$2b$") || hashWithSalt.StartsWith("$2y$"))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, hashWithSalt);
                }
                catch
                {
                    return false;
                }
            }

            string[] parts = hashWithSalt.Split('-');
            if (parts.Length != 2)
            {
                // Handle malformed hash string from database
                return false;
            }

            // Convert hash and salt from hex string back to byte arrays
            byte[] storedHash = Convert.FromHexString(parts[0]);
            byte[] salt = Convert.FromHexString(parts[1]);

            // 2. Re-hash the plain-text password using the EXTRACTED salt
            byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,       // Use the same iterations count!
                Algorithm,        // Use the same algorithm (SHA512)!
                KeySize);         // Use the same key size!

            // 3. Compare the two byte arrays (storedHash vs. computedHash)
            return computedHash.SequenceEqual(storedHash);
        }
    }


}
