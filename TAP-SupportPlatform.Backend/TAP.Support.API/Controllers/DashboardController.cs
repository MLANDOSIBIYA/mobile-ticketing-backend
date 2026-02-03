using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TAP.Support.Infrastructure.Data;
using System.Security.Claims;

namespace TAP.Support.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("client/{userId}")]
        public async Task<IActionResult> GetClientDashboard(Guid userId)
        {
            try
            {
                _logger.LogInformation($"Fetching dashboard for client: {userId}");

                // Get user
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.Role == "client");

                if (user == null)
                {
                    return NotFound(new { message = "Client not found" });
                }

                // Calculate stats using separate queries (EF-friendly)
                var totalTickets = await _context.Tickets
                    .CountAsync(t => t.ClientId == userId);

                var openTickets = await _context.Tickets
                    .CountAsync(t => t.ClientId == userId && t.Status != "closed");

                var inProgressTickets = await _context.Tickets
                    .CountAsync(t => t.ClientId == userId && t.Status == "in_progress");

                var resolvedTickets = await _context.Tickets
                    .CountAsync(t => t.ClientId == userId && t.Status == "closed");

                var pendingBookings = await _context.Bookings
                    .CountAsync(b => b.ClientId == userId && (b.Status == "pending" || b.Status == "confirmed"));

                // Get tickets for this client
                var tickets = await _context.Tickets
                    .Where(t => t.ClientId == userId)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(10)
                    .Select(t => new
                    {
                        t.Id,
                        t.TicketNumber,
                        t.Subject,
                        t.Description,
                        t.Status,
                        t.Priority,
                        t.Module,
                        t.CreatedAt,
                        t.UpdatedAt
                    })
                    .ToListAsync();

                // Get bookings for this client
                var bookings = await _context.Bookings
                    .Where(b => b.ClientId == userId)
                    .OrderByDescending(b => b.StartTime)
                    .Take(5)
                    .Select(b => new
                    {
                        b.Id,
                        b.ConsultantId,
                        b.StartTime,
                        b.EndTime,
                        b.ServiceType,
                        b.Status,
                        b.TotalAmount
                    })
                    .ToListAsync();

                var dashboardData = new
                {
                    success = true,
                    stats = new
                    {
                        totalTickets,
                        openTickets,
                        inProgressTickets,
                        resolvedTickets,
                        pendingBookings
                    },
                    recentTickets = tickets,
                    upcomingBookings = bookings,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        name = $"{user.FirstName} {user.LastName}",
                        role = user.Role,
                        phone = user.Phone,
                        tenantId = user.TenantId
                    }
                };

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching dashboard for client {userId}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error loading dashboard data",
                    error = ex.Message
                });
            }
        }

        // REMOVE THIS METHOD or make it static and don't use it in LINQ queries
        // If you need GetTimeAgo functionality, do it on the client-side or after data is fetched

        // Static version if you need it elsewhere (but not in LINQ)
        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalDays > 365)
                return $"{(int)(timeSpan.TotalDays / 365)} years ago";
            if (timeSpan.TotalDays > 30)
                return $"{(int)(timeSpan.TotalDays / 30)} months ago";
            if (timeSpan.TotalDays > 1)
                return $"{(int)timeSpan.TotalDays} days ago";
            if (timeSpan.TotalHours > 1)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalMinutes > 1)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";

            return "just now";
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            return Ok(new
            {
                message = "Dashboard controller is working",
                timestamp = DateTime.UtcNow
            });
        }
    }
}