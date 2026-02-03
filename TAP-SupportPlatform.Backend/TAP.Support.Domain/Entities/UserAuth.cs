using System.ComponentModel.DataAnnotations;

namespace TAP.Support.Domain.Entities
{
    public class UserAuth
    {
        public Guid UserId { get; set; }
        public User? User { get; set; } // Made nullable

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? MfaSecret { get; set; } // Made nullable

        public DateTime? LastLogin { get; set; }
        public int LoginAttempts { get; set; } = 0;
        public bool IsLocked { get; set; } = false;
    }
}