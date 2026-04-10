using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

// Concrete implementation of ITagRepository using EF Core.
// Tags are extracted from post content (hashtags like #dev) and stored as normalized entities.
public class TagRepository : ITagRepository
{
    private readonly AppDbContext _context;

    public TagRepository(AppDbContext context)
    {
        _context = context;
    }

    // Finds a tag by its exact name (case-sensitive at the DB level).
    // Used during post creation to check if a hashtag already exists before creating a new one.
    // FirstOrDefaultAsync — returns null if no tag matches (nullable return: Tag?).
    public async Task<Tag?> GetByNameAsync(string name)
    {
        return await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
    }

    // Returns all tags, ordered by popularity (most used first).
    // Include(t => t.PostTags) — eagerly loads the join table entries.
    // OrderByDescending(t => t.PostTags!.Count) — sorts by the number of posts using each tag.
    // Count is evaluated on the collection — EF Core translates this to a subquery COUNT.
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
