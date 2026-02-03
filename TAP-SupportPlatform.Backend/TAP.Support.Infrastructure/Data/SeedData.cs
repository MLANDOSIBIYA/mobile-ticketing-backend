using TAP.Support.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TAP.Support.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            if (await context.Tenants.AnyAsync()) return;

            // Create main tenant
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "SpecCon",
                Subdomain = "speccon",
                PrimaryColor = "#0066cc",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Tenants.AddAsync(tenant);

            // ===== CLIENT USERS (2 users) =====
            var client1 = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = "john@speccon.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "client",
                Phone = "+27 82 123 4567",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(client1);

            var client1Auth = new UserAuth
            {
                UserId = client1.Id,
                PasswordHash = "demo", // Plain text for testing
                LastLogin = null,
                LoginAttempts = 0,
                IsLocked = false
            };
            await context.UserAuths.AddAsync(client1Auth);

            var client2 = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = "sarah@speccon.com",
                FirstName = "Sarah",
                LastName = "Johnson",
                Role = "client",
                Phone = "+27 83 987 6543",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(client2);

            var client2Auth = new UserAuth
            {
                UserId = client2.Id,
                PasswordHash = "client123", // Plain text for testing
                LastLogin = null,
                LoginAttempts = 0,
                IsLocked = false
            };
            await context.UserAuths.AddAsync(client2Auth);

            // ===== CONSULTANT USERS (2 users) =====
            var consultant1 = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = "jane@speccon.com",
                FirstName = "Jane",
                LastName = "Williams",
                Role = "consultant",
                Phone = "+27 83 456 7890",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(consultant1);

            var consultant1Auth = new UserAuth
            {
                UserId = consultant1.Id,
                PasswordHash = "consultant123", // Plain text for testing
                LastLogin = null,
                LoginAttempts = 0,
                IsLocked = false
            };
            await context.UserAuths.AddAsync(consultant1Auth);

            var consultant2 = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = "robert@speccon.com",
                FirstName = "Robert",
                LastName = "Chen",
                Role = "consultant",
                Phone = "+27 84 321 0987",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(consultant2);

            var consultant2Auth = new UserAuth
            {
                UserId = consultant2.Id,
                PasswordHash = "consultant456", // Plain text for testing
                LastLogin = null,
                LoginAttempts = 0,
                IsLocked = false
            };
            await context.UserAuths.AddAsync(consultant2Auth);

            // ===== AGENT USERS (2 users) =====
            var agent1 = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = "mike@speccon.com",
                FirstName = "Mike",
                LastName = "Anderson",
                Role = "agent",
                Phone = "+27 85 654 3210",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(agent1);

            var agent1Auth = new UserAuth
            {
                UserId = agent1.Id,
                PasswordHash = "agent123", // Plain text for testing
                LastLogin = null,
                LoginAttempts = 0,
                IsLocked = false
            };
            await context.UserAuths.AddAsync(agent1Auth);

            var agent2 = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = "lisa@speccon.com",
                FirstName = "Lisa",
                LastName = "Martinez",
                Role = "agent",
                Phone = "+27 86 789 0123",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(agent2);

            var agent2Auth = new UserAuth
            {
                UserId = agent2.Id,
                PasswordHash = "agent456", // Plain text for testing
                LastLogin = null,
                LoginAttempts = 0,
                IsLocked = false
            };
            await context.UserAuths.AddAsync(agent2Auth);

            // ===== ADMIN USERS (2 users) =====
            var admin1 = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = "admin@speccon.com",
                FirstName = "Admin",
                LastName = "User",
                Role = "admin",
                Phone = "+27 84 999 8888",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(admin1);

            var admin1Auth = new UserAuth
            {
                UserId = admin1.Id,
                PasswordHash = "Admin@123", // Plain text for testing
                LastLogin = null,
                LoginAttempts = 0,
                IsLocked = false
            };
            await context.UserAuths.AddAsync(admin1Auth);

            var admin2 = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = "superadmin@speccon.com",
                FirstName = "Super",
                LastName = "Admin",
                Role = "admin",
                Phone = "+27 87 777 6666",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(admin2);

            var admin2Auth = new UserAuth
            {
                UserId = admin2.Id,
                PasswordHash = "SuperAdmin@123", // Plain text for testing
                LastLogin = null,
                LoginAttempts = 0,
                IsLocked = false
            };
            await context.UserAuths.AddAsync(admin2Auth);

            // ===== SAMPLE TICKETS (assigned to agents) =====
            var ticket1 = new Ticket
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                TicketNumber = "TKT-000001",
                ClientId = client1.Id,
                AssignedAgentId = agent1.Id,
                Subject = "Cannot generate EE report",
                Description = "When I try to generate the EE report for Q4, the system shows error 500.",
                Priority = "high",
                Status = "in_progress",
                Module = "Employment Equity",
                CreatedAt = DateTime.UtcNow.AddDays(-11),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };
            await context.Tickets.AddAsync(ticket1);

            var ticket2 = new Ticket
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                TicketNumber = "TKT-000002",
                ClientId = client2.Id,
                AssignedAgentId = agent2.Id,
                Subject = "Login issue",
                Description = "Unable to login with correct credentials",
                Priority = "medium",
                Status = "open",
                Module = "Authentication",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            };
            await context.Tickets.AddAsync(ticket2);

            // ===== SAMPLE BOOKINGS =====
            var booking1 = new Booking
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                ClientId = client1.Id,
                ConsultantId = consultant1.Id,
                StartTime = new DateTime(2025, 12, 15, 14, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 12, 15, 16, 0, 0, DateTimeKind.Utc),
                ServiceType = "EE Compliance Auditing",
                LocationType = "onsite",
                Address = "123 Business Park, Johannesburg, South Africa",
                TotalAmount = 500.00m,
                Status = "confirmed",
                CreatedAt = DateTime.UtcNow
            };
            await context.Bookings.AddAsync(booking1);

            var booking2 = new Booking
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                ClientId = client2.Id,
                ConsultantId = consultant2.Id,
                StartTime = new DateTime(2025, 12, 20, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2025, 12, 20, 12, 0, 0, DateTimeKind.Utc),
                ServiceType = "Training Session",
                LocationType = "online",
                Instructions = "Please join via Zoom link",
                TotalAmount = 300.00m,
                Status = "confirmed",
                CreatedAt = DateTime.UtcNow
            };
            await context.Bookings.AddAsync(booking2);

            await context.SaveChangesAsync();
        }
    }
}