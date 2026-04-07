using System.Net;
using System.Text.Json;

namespace Blog.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // Calls the next middleware/controller
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = ex switch
            {
                KeyNotFoundException => (int)HttpStatusCode.NotFound, // 404
                UnauthorizedAccessException =>(int)HttpStatusCode.Forbidden, // 403
                ArgumentException => (int)HttpStatusCode.BadRequest, // 400
                _ => (int)HttpStatusCode.InternalServerError // 500
            };

            var response = new { error = ex.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}