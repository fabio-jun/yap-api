using Blog.Domain.Interfaces;
using Blog.Application.Cache;
using Blog.Application.DTOs.Users;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

// Service that handles user profile operations: get, search, suggest, update.
public class UserService : IUserService
{
    private static readonly TimeSpan UserProfileTtl = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan SuggestedUsersTtl = TimeSpan.FromMinutes(10);

    private readonly IUserRepository _userRepository;
    private readonly IFollowRepository _followRepository;
    private readonly ICacheService _cache;

    public UserService(IUserRepository userRepository, IFollowRepository followRepository, ICacheService cache)
    {
        _userRepository = userRepository;
        _followRepository = followRepository;
        _cache = cache;
    }

    // Returns a user's public profile with computed follow counts.
    public async Task<UserResponse> GetByIdAsync(int id)
    {
        var cacheKey = CacheKeys.UserProfile(id);
        var cached = await _cache.GetAsync<UserResponse>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var user = await _userRepository.GetByIdAsync(id);
        if(user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // These counts are computed on each request (not stored on the User entity)
        var followersCount = await _followRepository.GetFollowersCountAsync(user.Id);
        var followingCount = await _followRepository.GetFollowingCountAsync(user.Id);

        // Map entity → DTO. Notice PasswordHash is never included in the response.
        var response = new UserResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            ProfileImageUrl = user.ProfileImageUrl,
            Bio = user.Bio,
            FollowersCount = followersCount,
            FollowingCount = followingCount
        };

        await _cache.SetAsync(cacheKey, response, UserProfileTtl);
        return response;
    }

    // Returns users the current user doesn't follow (for the "Suggested Users" sidebar).
    public async Task<IEnumerable<UserResponse>> GetSuggestedAsync(int userId, int count)
    {
        var cacheKey = CacheKeys.SuggestedUsers(userId);
        var cached = await _cache.GetAsync<List<UserResponse>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var users = await _userRepository.GetSuggestedAsync(userId, count);
        // .Select() with lambda — maps each User entity to a UserResponse DTO
        var result = users.Select(u => new UserResponse
        {
            Id = u.Id,
            UserName = u.UserName,
            DisplayName = u.DisplayName,
            Email = u.Email,
            Role = u.Role,
            CreatedAt = u.CreatedAt,
            ProfileImageUrl = u.ProfileImageUrl,
            Bio = u.Bio
        }).ToList();

        await _cache.SetAsync(cacheKey, result, SuggestedUsersTtl);
        return result;
    }

    // Searches users by username (case-insensitive via PostgreSQL ILike).
    public async Task<IEnumerable<UserResponse>> SearchAsync(string query)
    {
        var users = await _userRepository.SearchAsync(query);
        var responses = new List<UserResponse>();

        foreach (var user in users)
        {
            responses.Add(new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                Bio = user.Bio,
                // Each user needs separate COUNT queries for follower/following counts
                FollowersCount = await _followRepository.GetFollowersCountAsync(user.Id),
                FollowingCount = await _followRepository.GetFollowingCountAsync(user.Id)
            });
        }

        return responses;
    }

    // Updates the authenticated user's own profile.
    public async Task<UserResponse> UpdateAsync(int userId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if(user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Mutate the entity — EF Core's change tracker detects modifications
        user.UserName = request.userName;
        user.Email = request.Email;
        user.DisplayName = request.DisplayName;
        user.ProfileImageUrl = request.ProfileImageUrl;
        user.Bio = request.Bio;

        // UPDATE Users SET ... WHERE Id = userId
        await _userRepository.UpdateAsync(user);
        await _cache.RemoveAsync(CacheKeys.UserProfile(userId));
        await _cache.RemoveAsync(CacheKeys.SuggestedUsers(userId));

        var followersCount = await _followRepository.GetFollowersCountAsync(user.Id);
        var followingCount = await _followRepository.GetFollowingCountAsync(user.Id);

        return new UserResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            ProfileImageUrl = user.ProfileImageUrl,
            Bio = user.Bio,
            FollowersCount = followersCount,
            FollowingCount = followingCount
        };
    }
}
