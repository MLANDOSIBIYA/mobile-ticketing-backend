using Microsoft.EntityFrameworkCore;
using TAP.Support.Domain.Entities;

namespace TAP.Support.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserAuth> UserAuths { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tenant
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.Subdomain).IsUnique();
                entity.HasIndex(t => t.CustomDomain).IsUnique();
            });

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
                entity.HasIndex(u => new { u.TenantId, u.Role });

                // User → Tenant
                entity.HasOne(u => u.Tenant)
                      .WithMany(t => t.Users)
                      .HasForeignKey(u => u.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // UserAuth
            modelBuilder.Entity<UserAuth>(entity =>
            {
                entity.HasKey(ua => ua.UserId);

                entity.HasOne(ua => ua.User)
                      .WithOne(u => u.Auth)
                      .HasForeignKey<UserAuth>(ua => ua.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Ticket
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.TicketNumber).IsUnique();
                entity.HasIndex(t => new { t.TenantId, t.Status });
                entity.HasIndex(t => new { t.TenantId, t.Priority });

                // Ticket → Tenant
                entity.HasOne(t => t.Tenant)
                      .WithMany(tenant => tenant.Tickets)
                      .HasForeignKey(t => t.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Ticket → Client (User)
                entity.HasOne(t => t.Client)
                      .WithMany(u => u.CreatedTickets)
                      .HasForeignKey(t => t.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Ticket → Assigned Agent (User)
                entity.HasOne(t => t.AssignedAgent)
                      .WithMany(u => u.AssignedTickets)
                      .HasForeignKey(t => t.AssignedAgentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Booking
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.HasIndex(b => new { b.TenantId, b.StartTime });
                entity.HasIndex(b => new { b.ConsultantId, b.StartTime });

                // Booking → Tenant
                entity.HasOne(b => b.Tenant)
                      .WithMany(tenant => tenant.Bookings)
                      .HasForeignKey(b => b.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Booking → Client (User)
                entity.HasOne(b => b.Client)
                      .WithMany(u => u.ClientBookings)
                      .HasForeignKey(b => b.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Booking → Consultant (User)
                entity.HasOne(b => b.Consultant)
                      .WithMany()
                      .HasForeignKey(b => b.ConsultantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
