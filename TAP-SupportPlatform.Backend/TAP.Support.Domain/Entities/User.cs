using System.ComponentModel.DataAnnotations;

namespace TAP.Support.Domain.Entities
{
    public class User : BaseEntity  // Inherit from BaseEntity
    {
        // TenantId is now inherited from BaseEntity
        // public Guid TenantId { get; set; } // REMOVE THIS LINE
        // public Tenant? Tenant { get; set; } // Keep this navigation property

        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [Required, MaxLength(50)]
        public string Role { get; set; } = "client";

        [MaxLength(20)]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;
        // CreatedAt is now inherited
        // public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // REMOVE

        // Navigation properties
        public Tenant? Tenant { get; set; }
        public UserAuth? Auth { get; set; }

        // Tickets created by this user (Client)
        public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

        // Tickets assigned to this user (Agent)
        public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

        // Bookings where user is a client
        public ICollection<Booking> ClientBookings { get; set; } = new List<Booking>();

        // Bookings where user is a consultant
        public ICollection<Booking> ConsultantBookings { get; set; } = new List<Booking>();
    }
}