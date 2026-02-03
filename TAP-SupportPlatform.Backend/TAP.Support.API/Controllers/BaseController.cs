using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TAP.Support.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All derived controllers require authentication
    public abstract class BaseController : ControllerBase
    {
        protected Guid CurrentTenantId
        {
            get
            {
                var tenantIdClaim = User.FindFirst("tenantId")?.Value;
                if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
                {
                    throw new UnauthorizedAccessException("Tenant ID not found in token");
                }
                return tenantId;
            }
        }

        protected Guid CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    throw new UnauthorizedAccessException("User ID not found in token");
                }
                return userId;
            }
        }

        protected string CurrentUserRole => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        protected bool IsAdmin => CurrentUserRole == "admin";
        protected bool IsConsultant => CurrentUserRole == "consultant";
        protected bool IsAgent => CurrentUserRole == "agent";
        protected bool IsClient => CurrentUserRole == "client";

        // Combined permissions
        protected bool CanManageTickets => IsAdmin || IsAgent;
        protected bool CanManageUsers => IsAdmin;
        protected bool CanManageBookings => IsAdmin || IsConsultant || IsAgent;
        protected bool CanViewAllTickets => IsAdmin || IsAgent;
    }
}