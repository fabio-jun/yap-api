using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Comment>> GetByPostIdAsync(int postId)
    {
        return await _context.Comments
            .Where(c => c.PostId == postId)
            .Include(c => c.Author)
            .ToListAsync();
    }

    public async Task<Comment?> GetByIdAsync(int id)
    {
        return await _context.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Comment comment)
    {
         _context.Comments.Remove(comment);
         await _context.SaveChangesAsync();
    }
}

