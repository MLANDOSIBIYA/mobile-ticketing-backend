using Microsoft.Extensions.Logging;
using TAP.Support.Domain.Interfaces;

namespace TAP.Support.Infrastructure.Services
{
    public class SimplePasswordHasher : IPasswordHasher
    {
        private readonly ILogger<SimplePasswordHasher> _logger;

        public SimplePasswordHasher(ILogger<SimplePasswordHasher> logger)
        {
            _logger = logger;
        }

        public string Hash(string password)
        {
            _logger.LogDebug("Hashing (plain text): {Password}", password);
            return password; // Return plain text for testing
        }

        public bool Verify(string password, string hash)
        {
            _logger.LogDebug("Verifying password: {Password} against hash: {Hash}", password, hash);
            return password == hash; // Simple comparison for testing
        }
    }
}