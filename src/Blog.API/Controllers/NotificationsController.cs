using System.Security.Claims;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get notifications", Description = "Returns paged notifications for the authenticated user.")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _notificationService.GetNotificationsAsync(userId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("recent")]
    [SwaggerOperation(Summary = "Get recent notifications", Description = "Returns the most recent notifications for the dropdown.")]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 8)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _notificationService.GetRecentAsync(userId, count);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    [SwaggerOperation(Summary = "Get unread notification count", Description = "Returns the number of unread notifications for the authenticated user.")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(count);
    }

    [HttpPut("{id}/read")]
    [SwaggerOperation(Summary = "Mark notification as read", Description = "Marks one notification as read for the authenticated user.")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _notificationService.MarkAsReadAsync(id, userId);
        return NoContent();
    }

    [HttpPut("read-all")]
    [SwaggerOperation(Summary = "Mark all notifications as read", Description = "Marks every notification as read for the authenticated user.")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete notification", Description = "Deletes one notification owned by the authenticated user.")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _notificationService.DeleteAsync(id, userId);
        return NoContent();
    }
}
