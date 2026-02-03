using System.ComponentModel.DataAnnotations;

namespace TAP.Support.Domain.Entities
{
    public class Ticket : BaseEntity
    {
        [Required, MaxLength(50)]
        public string TicketNumber { get; set; } = string.Empty;

        [Required]
        public Guid ClientId { get; set; }
        public User? Client { get; set; }

        public Guid? AssignedAgentId { get; set; }
        public User? AssignedAgent { get; set; }

        [Required, MaxLength(500)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Priority { get; set; } = "medium";

        [MaxLength(50)]
        public string Status { get; set; } = "open";

        [MaxLength(100)]
        public string? Module { get; set; }

        [MaxLength(100)]
        public string? Feature { get; set; }

        public string? AttachmentUrl { get; set; }

        // UpdatedAt is already nullable in BaseEntity, no need to redeclare

        // Navigation property
        public Tenant? Tenant { get; set; }
    }
}