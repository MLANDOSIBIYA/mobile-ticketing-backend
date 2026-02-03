using System.ComponentModel.DataAnnotations;

namespace TAP.Support.Domain.Entities
{
    public class Booking : BaseEntity  // Inherit from BaseEntity
    {
        // TenantId is now inherited from BaseEntity
        // public Guid TenantId { get; set; } // REMOVE THIS LINE
        // public Tenant? Tenant { get; set; } // Keep this navigation property

        [Required]
        public Guid ClientId { get; set; }
        public User? Client { get; set; }

        [Required]
        public Guid ConsultantId { get; set; }
        public User? Consultant { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [Required, MaxLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string LocationType { get; set; } = "online";

        public string? Address { get; set; }
        public string? Instructions { get; set; }

        public decimal TotalAmount { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "confirmed";

        // CreatedAt is now inherited
        // public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // REMOVE

        // Navigation property (keep this)
        public Tenant? Tenant { get; set; }
    }
}