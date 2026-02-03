using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TAP.Support.Infrastructure.Data;
using TAP.Support.Domain.Entities;
using TAP.Support.Domain.Interfaces;

namespace TAP.Support.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IPasswordHasher _passwordHasher;

        public AuthController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IPasswordHasher passwordHasher)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", request.Email);

                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { message = "Email and password are required" });
                }

                var user = await _context.Users
                    .Include(u => u.Auth)
                    .Include(u => u.Tenant)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (user == null || user.Auth == null)
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new { message = "Account is inactive" });
                }

                if (user.Tenant == null || !user.Tenant.IsActive)
                {
                    return Unauthorized(new { message = "Tenant is not active" });
                }

                if (user.Auth.IsLocked)
                {
                    return Unauthorized(new { message = "Account is locked" });
                }

                if (!_passwordHasher.Verify(request.Password, user.Auth.PasswordHash))
                {
                    user.Auth.LoginAttempts++;

                    if (user.Auth.LoginAttempts >= 5)
                        user.Auth.IsLocked = true;

                    await _context.SaveChangesAsync();
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                user.Auth.LoginAttempts = 0;
                user.Auth.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    success = true,
                    token,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        fullName = $"{user.FirstName} {user.LastName}",
                        role = user.Role,
                        tenantId = user.TenantId,
                        tenantName = user.Tenant?.Name,
                        phone = user.Phone
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                return StatusCode(500, new { message = "Login failed" });
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.TenantSubdomain))
                {
                    return BadRequest(new { message = "Missing required fields" });
                }

                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.Subdomain == request.TenantSubdomain && t.IsActive);

                if (tenant == null)
                    return BadRequest(new { message = "Tenant not found" });

                var exists = await _context.Users
                    .AnyAsync(u => u.Email == request.Email && u.TenantId == tenant.Id);

                if (exists)
                    return Conflict(new { message = "User already exists" });

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Role = "client",
                    Phone = request.Phone,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var auth = new UserAuth
                {
                    UserId = user.Id,
                    PasswordHash = request.Password,
                    LoginAttempts = 0,
                    IsLocked = false
                };

                await _context.Users.AddAsync(user);
                await _context.UserAuths.AddAsync(auth);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    success = true,
                    token,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        role = user.Role,
                        tenantId = user.TenantId,
                        tenantName = tenant.Name,
                        phone = user.Phone
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register error");
                return StatusCode(500, new { message = "Registration failed" });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("tenantId", user.TenantId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["Jwt:ExpiryMinutes"]!)
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string TenantSubdomain { get; set; } = "";
    }
}
