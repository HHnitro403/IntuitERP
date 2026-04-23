using BCrypt.Net;
using System;

namespace IntuiERP.Avalonia.UI.Services
{
    /// <summary>
    /// Service for securely hashing and verifying passwords using BCrypt
    /// </summary>
    public class PasswordHashingService
    {
        // BCrypt work factor (cost). Higher = more secure but slower
        // 12 is a good balance for 2024
        private const int WorkFactor = 12;

        /// <summary>
        /// Hashes a plain text password using BCrypt
        /// </summary>
        /// <param name="plainTextPassword">The password to hash</param>
        /// <returns>Hashed password string</returns>
        public string HashPassword(string plainTextPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(plainTextPassword));
            }

            // BCrypt automatically generates a salt and includes it in the hash
            return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, WorkFactor);
        }

        /// <summary>
        /// Verifies a plain text password against a hashed password
        /// </summary>
        /// <param name="plainTextPassword">The password to verify</param>
        /// <param name="hashedPassword">The hashed password to compare against</param>
        /// <returns>True if password matches, false otherwise</returns>
        public bool VerifyPassword(string plainTextPassword, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                return false;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(plainTextPassword, hashedPassword);
            }
            catch (Exception)
            {
                // If hash is invalid format, return false
                return false;
            }
        }

        /// <summary>
        /// Checks if a password needs to be rehashed (e.g., if work factor has changed)
        /// </summary>
        /// <param name="hashedPassword">The hashed password to check</param>
        /// <returns>True if password should be rehashed</returns>
        public bool NeedsRehash(string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                return true;
            }

            try
            {
                return BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, WorkFactor);
            }
            catch (Exception)
            {
                // If hash is invalid, it needs rehashing
                return true;
            }
        }

        /// <summary>
        /// Checks if a string appears to be a BCrypt hash
        /// </summary>
        /// <param name="password">String to check</param>
        /// <returns>True if it looks like a BCrypt hash</returns>
        public bool IsPasswordHashed(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            // BCrypt hashes start with $2a$, $2b$, or $2y$ and are 60 characters long
            return password.StartsWith("$2") && password.Length == 60;
        }
    }
}
