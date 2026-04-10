using System.Net;
using System.Text.Json;

namespace Blog.API.Middlewares;

// Global exception handler middleware.
// Middleware sits in the HTTP pipeline and processes every request/response.
// This catches unhandled exceptions thrown by controllers/services and converts them
// into structured JSON error responses with appropriate HTTP status codes.
public class ExceptionMiddleware
{
    // RequestDelegate — represents the next middleware in the pipeline.
    // readonly — can only be assigned in the constructor.
    private readonly RequestDelegate _next;

    // Constructor receives the next middleware via DI.
    // ASP.NET Core calls this once when building the pipeline.
    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // InvokeAsync — called for every HTTP request passing through this middleware.
    // HttpContext — contains all HTTP request/response data (headers, body, route, user, etc.).
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Calls the next middleware or controller in the pipeline.
            // If no exception occurs, the response flows back normally.
            await _next(context);
        }
        catch (Exception ex)
        {
            // Set response content type to JSON for the error body.
            context.Response.ContentType = "application/json";

            // Switch expression — maps exception types to HTTP status codes.
            // Pattern matching: each case checks if 'ex' is of a specific type.
            // '_' is the discard pattern (default/fallback case).
            context.Response.StatusCode = ex switch
            {
                KeyNotFoundException => (int)HttpStatusCode.NotFound,             // 404
                UnauthorizedAccessException => (int)HttpStatusCode.Forbidden,     // 403
                ArgumentException => (int)HttpStatusCode.BadRequest,              // 400
                _ => (int)HttpStatusCode.InternalServerError                      // 500
            };

            // Anonymous object — creates a { error: "message" } JSON response.
            // JsonSerializer.Serialize — converts the object to a JSON string.
            var response = new { error = ex.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
