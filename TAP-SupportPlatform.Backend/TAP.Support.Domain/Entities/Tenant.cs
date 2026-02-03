using System.ComponentModel.DataAnnotations;

namespace TAP.Support.Domain.Entities
{
    public class Tenant
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Subdomain { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? CustomDomain { get; set; } // Made nullable

        public string? LogoUrl { get; set; } // Made nullable

        [MaxLength(7)]
        public string PrimaryColor { get; set; } = "#0066cc";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}