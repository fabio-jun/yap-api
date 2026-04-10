using Blog.Application.DTOs.DirectMessages;
using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Blog.API.Controllers;

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

    // GET api/messages — returns conversation list (inbox)
    [HttpGet]
    public async Task<IActionResult> GetConversations()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var conversations = await _messageService.GetConversationsAsync(userId);
        return Ok(conversations);
    }

    // GET api/messages/{userId} — returns all messages with a specific user
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetConversation(int userId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var messages = await _messageService.GetConversationAsync(currentUserId, userId);
        return Ok(messages);
    }

    // POST api/messages/{userId} — send a message to a user
    [HttpPost("{userId}")]
    public async Task<IActionResult> Send(int userId, SendMessageRequest request)
    {
        var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var message = await _messageService.SendAsync(senderId, userId, request);
        return Ok(message);
    }

    // DELETE api/messages/msg/{id} — delete a message (sender only)
    [HttpDelete("msg/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _messageService.DeleteAsync(id, userId);
        return NoContent();
    }
}
