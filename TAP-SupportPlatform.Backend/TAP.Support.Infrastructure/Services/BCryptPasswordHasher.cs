using Microsoft.Extensions.Logging;
using TAP.Support.Domain.Interfaces;

namespace TAP.Support.Infrastructure.Services
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        private readonly ILogger<BCryptPasswordHasher> _logger;

        public BCryptPasswordHasher(ILogger<BCryptPasswordHasher> logger)
        {
            _logger = logger;
        }

        public string Hash(string password)
        {
            try
            {
                // Correct BCrypt.Net-Next usage
                return BCrypt.Net.BCrypt.HashPassword(password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                throw;
            }
        }

        public bool Verify(string password, string hash)
        {
            try
            {
                if (string.IsNullOrEmpty(hash))
                    return false;

                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password hash");
                return false;
            }
        }
    }
}