using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

// Handles private messaging (DM) between users
public class DirectMessageRepository : IDirectMessageRepository
{
    private readonly AppDbContext _context;

    public DirectMessageRepository(AppDbContext context)
    {
        _context = context;
    }

    // Returns all messages between two specific users, ordered chronologically.
    // The Where clause matches messages in both directions:
    //   (sender=user1 AND receiver=user2) OR (sender=user2 AND receiver=user1)
    // This gives a complete bidirectional conversation thread.
    // OrderBy shows messages from oldest to newest.
    public async Task<IEnumerable<DirectMessage>> GetConversationAsync(int userId1, int userId2)
    {
        return await _context.DirectMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2)
                     || (m.SenderId == userId2 && m.ReceiverId == userId1))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    // Returns a preview of all conversations for a user (inbox view).
    // Strategy: load all messages involving the user, then group by the other participant.
    //
    // GroupBy logic:
    //   m.SenderId == userId ? m.ReceiverId : m.SenderId
    //   — determines who the "other person" is in each message (ternary operator)
    //
    // Select(g => g.First()) — takes only the most recent message per conversation
    //   (messages are already sorted by CreatedAt DESC before grouping).
    public async Task<IEnumerable<DirectMessage>> GetConversationsListAsync(int userId)
    {
        var messages = await _context.DirectMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => g.First())
            .ToList();
    }

    // Returns a specific message by its ID, including sender and receiver data.
    public async Task<DirectMessage?> GetByIdAsync(int id)
    {
        return await _context.DirectMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddAsync(DirectMessage message)
    {
        await _context.DirectMessages.AddAsync(message);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(DirectMessage message)
    {
        _context.DirectMessages.Remove(message);
        await _context.SaveChangesAsync();
    }
}
