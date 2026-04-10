using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class DirectMessageRepository : IDirectMessageRepository
{
    private readonly AppDbContext _context;

    public DirectMessageRepository(AppDbContext context)
    {
        _context = context;
    }

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
