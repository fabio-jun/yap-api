using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

// Concrete implementation of IFollowRepository using EF Core.
// Manages the self-referencing many-to-many relationship: User follows User.
// Follow uses composite PK (FollowerId, FollowingId).
public class FollowRepository : IFollowRepository
{
    private readonly AppDbContext _context;

    public FollowRepository(AppDbContext context)
    {
        _context = context;
    }

    // Checks if a follow relationship exists between two users.
    // Used by the service layer for the toggle pattern (follow/unfollow).
    public async Task<Follow?> GetAsync(int followerId, int followingId)
    {
        return await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }

    // Returns all users who follow a given user (their followers).
    // Select(f => f.Follower!) — projects the query to return User entities instead of Follow entities.
    // The '!' (null-forgiving operator) tells the compiler we know Follower won't be null here.
    public async Task<IEnumerable<User>> GetFollowersAsync(int userId)
    {
        return await _context.Follows
            .Where(f => f.FollowingId == userId)
            .Select(f => f.Follower!)
            .ToListAsync();
    }

    // Returns all users that a given user follows.
    // Mirror of GetFollowersAsync but from the opposite direction of the relationship.
    public async Task<IEnumerable<User>> GetFollowingAsync(int userId)
    {
        return await _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.Following!)
            .ToListAsync();
    }

    // CountAsync with a predicate — executes SELECT COUNT(*) with a WHERE clause.
    // More efficient than loading entities just to count them.
    public async Task<int> GetFollowersCountAsync(int userId)
    {
        return await _context.Follows.CountAsync(f => f.FollowingId == userId);
    }

    public async Task<int> GetFollowingCountAsync(int userId)
    {
        return await _context.Follows.CountAsync(f => f.FollowerId == userId);
    }

    public async Task AddAsync(Follow follow)
    {
        await _context.Follows.AddAsync(follow);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Follow follow)
    {
        _context.Follows.Remove(follow);
        await _context.SaveChangesAsync();
    }
}
