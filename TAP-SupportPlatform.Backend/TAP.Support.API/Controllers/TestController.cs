// In your backend, create or update TestController.cs
using Microsoft.AspNetCore.Mvc;

namespace TAP.Support.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                Status = "Backend is running",
                Timestamp = DateTime.UtcNow,
                Message = "Hello from backend!",
                Version = "1.0"
            });
        }
    }
}