using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TAP.Support.Domain.Entities;
using TAP.Support.Infrastructure.Data;

namespace TAP.Support.API.Controllers
{
    public class TicketsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TicketsController> _logger;
        private readonly IWebHostEnvironment _environment;

        public TicketsController(
            ApplicationDbContext context,
            ILogger<TicketsController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        // GET: api/tickets
        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            try
            {
                // Only get tickets for current tenant
                var tickets = await _context.Tickets
                    .Include(t => t.Client)
                    .Include(t => t.AssignedAgent)
                    .Where(t => t.TenantId == CurrentTenantId)
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => new TicketResponse
                    {
                        Id = t.Id,
                        TicketNumber = t.TicketNumber,
                        Subject = t.Subject,
                        Description = t.Description,
                        Priority = t.Priority,
                        Status = t.Status,
                        Module = t.Module,
                        Feature = t.Feature,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt ?? t.CreatedAt, // Handle nullable
                        ClientId = t.ClientId,
                        ClientName = t.Client != null ? $"{t.Client.FirstName} {t.Client.LastName}" : "",
                        AssignedAgentId = t.AssignedAgentId,
                        AssignedAgentName = t.AssignedAgent != null ? $"{t.AssignedAgent.FirstName} {t.AssignedAgent.LastName}" : "",
                        AttachmentUrl = t.AttachmentUrl
                    })
                    .ToListAsync();

                return Ok(tickets);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in GetTickets");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets for tenant {TenantId}", CurrentTenantId);
                return StatusCode(500, new { message = "An error occurred while getting tickets" });
            }
        }

        // GET: api/tickets/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(Guid id)
        {
            try
            {
                // Ensure ticket belongs to current tenant
                var ticket = await _context.Tickets
                    .Include(t => t.Client)
                    .Include(t => t.AssignedAgent)
                    .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == CurrentTenantId);

                if (ticket == null)
                {
                    return NotFound(new { message = "Ticket not found" });
                }

                var response = new TicketResponse
                {
                    Id = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    Subject = ticket.Subject,
                    Description = ticket.Description,
                    Priority = ticket.Priority,
                    Status = ticket.Status,
                    Module = ticket.Module,
                    Feature = ticket.Feature,
                    CreatedAt = ticket.CreatedAt,
                    UpdatedAt = ticket.UpdatedAt ?? ticket.CreatedAt, // Handle nullable
                    ClientId = ticket.ClientId,
                    ClientName = ticket.Client != null ? $"{ticket.Client.FirstName} {ticket.Client.LastName}" : "",
                    AssignedAgentId = ticket.AssignedAgentId,
                    AssignedAgentName = ticket.AssignedAgent != null ? $"{ticket.AssignedAgent.FirstName} {ticket.AssignedAgent.LastName}" : "",
                    AttachmentUrl = ticket.AttachmentUrl
                };

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in GetTicketById");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket {TicketId} for tenant {TenantId}", id, CurrentTenantId);
                return StatusCode(500, new { message = "An error occurred while getting the ticket" });
            }
        }

        // POST: api/tickets (with file upload support)
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateTicket([FromForm] CreateTicketRequest request)
        {
            try
            {
                _logger.LogInformation("Creating ticket for user: {UserId}", CurrentUserId);

                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.Subject))
                    return BadRequest(new { message = "Subject is required" });

                if (string.IsNullOrWhiteSpace(request.Description))
                    return BadRequest(new { message = "Description is required" });

                string? attachmentUrl = null;

                // Handle file upload if exists
                if (request.File != null && request.File.Length > 0)
                {
                    // Validate file size (max 10MB)
                    if (request.File.Length > 10 * 1024 * 1024)
                        return BadRequest(new { message = "File size must be less than 10MB" });

                    // Validate file extension
                    var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".pdf", ".doc", ".docx" };
                    var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                        return BadRequest(new { message = "Invalid file type. Allowed: PNG, JPG, GIF, PDF, DOC" });

                    // Create uploads directory if it doesn't exist
                    var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "tickets");
                    if (!Directory.Exists(uploadsPath))
                        Directory.CreateDirectory(uploadsPath);

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.File.CopyToAsync(stream);
                    }

                    // Generate URL for accessing the file
                    attachmentUrl = $"/uploads/tickets/{fileName}";
                    _logger.LogInformation("File uploaded: {FilePath}", attachmentUrl);
                }

                // Create new ticket
                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    TenantId = CurrentTenantId,
                    TicketNumber = GenerateTicketNumber(),
                    ClientId = CurrentUserId,
                    Subject = request.Subject,
                    Description = request.Description,
                    Priority = request.Priority ?? "medium",
                    Status = "open",
                    Module = request.Module,
                    Feature = request.Category,
                    AttachmentUrl = attachmentUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Tickets.AddAsync(ticket);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Ticket created: {TicketNumber} by {UserId}", ticket.TicketNumber, CurrentUserId);

                // Return response without circular references
                var response = new TicketResponse
                {
                    Id = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    Subject = ticket.Subject,
                    Description = ticket.Description,
                    Priority = ticket.Priority,
                    Status = ticket.Status,
                    Module = ticket.Module,
                    Feature = ticket.Feature,
                    AttachmentUrl = ticket.AttachmentUrl,
                    CreatedAt = ticket.CreatedAt,
                    UpdatedAt = ticket.UpdatedAt ?? ticket.CreatedAt,
                    ClientId = ticket.ClientId,
                    ClientName = "Current User" // We can fetch this later if needed
                };

                return CreatedAtAction(nameof(GetTicketById), new { id = ticket.Id }, response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in CreateTicket");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket for tenant {TenantId}", CurrentTenantId);
                return StatusCode(500, new { message = "An error occurred while creating the ticket" });
            }
        }

        // PUT: api/tickets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(Guid id, [FromBody] UpdateTicketRequest request)
        {
            try
            {
                // Ensure ticket belongs to current tenant
                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == CurrentTenantId);

                if (ticket == null)
                {
                    return NotFound(new { message = "Ticket not found" });
                }

                // Update fields
                if (!string.IsNullOrEmpty(request.Subject))
                    ticket.Subject = request.Subject;

                if (!string.IsNullOrEmpty(request.Description))
                    ticket.Description = request.Description;

                if (!string.IsNullOrEmpty(request.Priority))
                    ticket.Priority = request.Priority;

                if (!string.IsNullOrEmpty(request.Status))
                    ticket.Status = request.Status;

                if (request.AssignedAgentId.HasValue)
                    ticket.AssignedAgentId = request.AssignedAgentId;

                ticket.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Return updated ticket
                var updatedTicket = await _context.Tickets
                    .Include(t => t.Client)
                    .Include(t => t.AssignedAgent)
                    .FirstOrDefaultAsync(t => t.Id == id);

                var response = new TicketResponse
                {
                    Id = updatedTicket!.Id,
                    TicketNumber = updatedTicket.TicketNumber,
                    Subject = updatedTicket.Subject,
                    Description = updatedTicket.Description,
                    Priority = updatedTicket.Priority,
                    Status = updatedTicket.Status,
                    CreatedAt = updatedTicket.CreatedAt,
                    UpdatedAt = updatedTicket.UpdatedAt ?? updatedTicket.CreatedAt,
                    ClientId = updatedTicket.ClientId,
                    ClientName = updatedTicket.Client != null ? $"{updatedTicket.Client.FirstName} {updatedTicket.Client.LastName}" : "",
                    AssignedAgentId = updatedTicket.AssignedAgentId,
                    AssignedAgentName = updatedTicket.AssignedAgent != null ? $"{updatedTicket.AssignedAgent.FirstName} {updatedTicket.AssignedAgent.LastName}" : ""
                };

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in UpdateTicket");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket {TicketId} for tenant {TenantId}", id, CurrentTenantId);
                return StatusCode(500, new { message = "An error occurred while updating the ticket" });
            }
        }

        private string GenerateTicketNumber()
        {
            var lastTicket = _context.Tickets
                .Where(t => t.TenantId == CurrentTenantId)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastTicket != null && lastTicket.TicketNumber.StartsWith("TKT-"))
            {
                var parts = lastTicket.TicketNumber.Split('-');
                if (parts.Length > 1 && int.TryParse(parts[1], out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            return $"TKT-{nextNumber:D6}";
        }
    }

    public class CreateTicketRequest
    {
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Priority { get; set; } = "medium";
        public string? Module { get; set; }
        public string? Category { get; set; }
        public IFormFile? File { get; set; }
    }

    public class UpdateTicketRequest
    {
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public Guid? AssignedAgentId { get; set; }
    }

    public class TicketResponse
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "medium";
        public string Status { get; set; } = "open";
        public string? Module { get; set; }
        public string? Feature { get; set; }
        public string? AttachmentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public Guid? AssignedAgentId { get; set; }
        public string? AssignedAgentName { get; set; }
    }
}