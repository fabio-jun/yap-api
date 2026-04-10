using Blog.Application.DTOs.DirectMessages;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Blog.API.Controllers;

// API controller for direct messaging (DM) between users.
// [Authorize] at class level — all DM endpoints require authentication.
// Route: api/messages (not api/directmessages — uses a custom route for cleaner URLs).
[ApiController]
[Route("api/messages")]
[Authorize]
public class DirectMessagesController : ControllerBase
{
    private readonly IDirectMessageService _messageService;

    public DirectMessagesController(IDirectMessageService messageService)
    {
        _messageService = messageService;
    }

    // GET api/messages — returns the conversation list (inbox view).
    // Shows the most recent message from each conversation partner.
    [HttpGet]
    public async Task<IActionResult> GetConversations()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var conversations = await _messageService.GetConversationsAsync(userId);
        return Ok(conversations);
    }

    // GET api/messages/{userId} — returns the full message history with a specific user.
    // {userId} here is the other participant in the conversation (not the current user).
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetConversation(int userId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var messages = await _messageService.GetConversationAsync(currentUserId, userId);
        return Ok(messages);
    }

    // POST api/messages/{userId} — sends a new message to the specified user.
    // SendMessageRequest (from body) contains the message content.
    [HttpPost("{userId}")]
    public async Task<IActionResult> Send(int userId, SendMessageRequest request)
    {
        var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var message = await _messageService.SendAsync(senderId, userId, request);
        return Ok(message);
    }

    // DELETE api/messages/msg/{id} — deletes a specific message (sender only).
    // "msg/{id}" avoids route conflict with GET {userId} (both would match an int).
    [HttpDelete("msg/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _messageService.DeleteAsync(id, userId);
        // NoContent() — HTTP 204, standard for successful DELETE.
        return NoContent();
    }
}
