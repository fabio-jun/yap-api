using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.API.Controllers;

// Health check endpoint — used by deployment platforms (Render, load balancers)
// to verify the API is running and responsive.
// Returns a simple JSON response with status and timestamp.
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    // GET api/health — returns HTTP 200 if the API is alive.
    // IActionResult (not Task<IActionResult>) — synchronous because there's no async work.
    // No database call — this is intentionally lightweight to avoid false negatives from DB issues.
    [HttpGet]
    [SwaggerOperation(Summary = "Health check", Description = "Returns a lightweight API health response with the current UTC timestamp.")]
    public IActionResult Get()
    {
        // Anonymous object with two properties, serialized to JSON.
        // DateTime.UtcNow — server timestamp in UTC (avoids timezone issues).
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow
        });
    }
}
