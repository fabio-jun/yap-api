using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
    private readonly AppDbContext _context;

    public TagRepository(AppDbContext context)  
    {
        _context = context;
    }

    // Finds a tag by its name
    // FirstOrDefaultAsync: return the first match or null if not found
    public async Task<Tag?> GetByNameAsync(string name)
    {
        return await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
    }

    // Returns all tags ordered by the number of posts that use them
    // Count() counts how many PostTags each tag has
    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        return await _context.Tags
            .Include(t => t.PostTags)
            .OrderByDescending(t => t.PostTags!.Count)
            .ToListAsync();
    }

    public async Task AddAsync(Tag tag)
    {
        await _context.Tags.AddAsync(tag);
        await _context.SaveChangesAsync();
    }
}