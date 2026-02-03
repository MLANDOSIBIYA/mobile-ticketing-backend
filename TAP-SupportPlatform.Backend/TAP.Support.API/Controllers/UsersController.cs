using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TAP.Support.Infrastructure.Data;

namespace TAP.Support.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            ApplicationDbContext context,
            ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                // Only admin can view all users
                if (!IsAdmin)
                {
                    return Forbid();
                }

                var users = await _context.Users
                    .Include(u => u.Tenant)
                    .Where(u => u.TenantId == CurrentTenantId)
                    .OrderBy(u => u.Role)
                    .ThenBy(u => u.LastName)
                    .Select(u => new UserResponse
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = $"{u.FirstName} {u.LastName}",
                        Role = u.Role,
                        Phone = u.Phone,
                        IsActive = u.IsActive,
                        TenantName = u.Tenant != null ? u.Tenant.Name : "Unknown",
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in GetUsers");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for tenant {TenantId}", CurrentTenantId);
                return StatusCode(500, new { message = "An error occurred while getting users" });
            }
        }

        // GET: api/users/roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetUsersByRole()
        {
            try
            {
                // Only admin and agents can view users by role
                if (!IsAdmin && !IsAgent)
                {
                    return Forbid();
                }

                var users = await _context.Users
                    .Where(u => u.TenantId == CurrentTenantId && u.IsActive)
                    .GroupBy(u => u.Role)
                    .Select(g => new
                    {
                        Role = g.Key,
                        Count = g.Count(),
                        Users = g.Select(u => new
                        {
                            u.Id,
                            u.Email,
                            Name = $"{u.FirstName} {u.LastName}",
                            u.Phone
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role for tenant {TenantId}", CurrentTenantId);
                return StatusCode(500, new { message = "An error occurred while getting users by role" });
            }
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            try
            {
                // Users can view their own profile, admins can view any
                if (!IsAdmin && CurrentUserId != id)
                {
                    return Forbid();
                }

                var user = await _context.Users
                    .Include(u => u.Tenant)
                    .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == CurrentTenantId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var response = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Role = user.Role,
                    Phone = user.Phone,
                    IsActive = user.IsActive,
                    TenantName = user.Tenant != null ? user.Tenant.Name : "Unknown",
                    CreatedAt = user.CreatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId} for tenant {TenantId}", id, CurrentTenantId);
                return StatusCode(500, new { message = "An error occurred while getting the user" });
            }
        }

        // GET: api/users/me
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Tenant)
                    .FirstOrDefaultAsync(u => u.Id == CurrentUserId && u.TenantId == CurrentTenantId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var response = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Role = user.Role,
                    Phone = user.Phone,
                    IsActive = user.IsActive,
                    TenantName = user.Tenant != null ? user.Tenant.Name : "Unknown",
                    CreatedAt = user.CreatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user for tenant {TenantId}", CurrentTenantId);
                return StatusCode(500, new { message = "An error occurred while getting user profile" });
            }
        }

        // GET: api/users/agents
        [HttpGet("agents")]
        public async Task<IActionResult> GetAgents()
        {
            try
            {
                // Anyone can view agents (for ticket assignment dropdowns)
                var agents = await _context.Users
                    .Where(u => u.TenantId == CurrentTenantId &&
                                u.Role == "agent" &&
                                u.IsActive)
                    .OrderBy(u => u.LastName)
                    .Select(u => new
                    {
                        u.Id,
                        Name = $"{u.FirstName} {u.LastName}",
                        u.Email,
                        u.Phone
                    })
                    .ToListAsync();

                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agents for tenant {TenantId}", CurrentTenantId);
                return StatusCode(500, new { message = "An error occurred while getting agents" });
            }
        }

        // GET: api/users/consultants
        [HttpGet("consultants")]
        public async Task<IActionResult> GetConsultants()
        {
            try
            {
                var consultants = await _context.Users
                    .Where(u => u.TenantId == CurrentTenantId &&
                                u.Role == "consultant" &&
                                u.IsActive)
                    .OrderBy(u => u.LastName)
                    .Select(u => new
                    {
                        u.Id,
                        Name = $"{u.FirstName} {u.LastName}",
                        u.Email,
                        u.Phone,
                        Specialization = "General Consultant" // You can add this field to User entity later
                    })
                    .ToListAsync();

                return Ok(consultants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting consultants for tenant {TenantId}", CurrentTenantId);
                return StatusCode(500, new { message = "An error occurred while getting consultants" });
            }
        }
    }

    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "client";
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}