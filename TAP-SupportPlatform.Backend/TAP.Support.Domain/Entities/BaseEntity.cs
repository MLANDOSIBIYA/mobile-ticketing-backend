using System.ComponentModel.DataAnnotations;

namespace TAP.Support.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } // Make nullable
    }
}