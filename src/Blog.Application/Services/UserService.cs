using Blog.Domain.Interfaces;
using Blog.Application.DTOs.Users;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

// Service that handles user profile operations: get, search, suggest, update.
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IFollowRepository _followRepository;

    public UserService(IUserRepository userRepository, IFollowRepository followRepository)
    {
        _userRepository = userRepository;
        _followRepository = followRepository;
    }

    // Returns a user's public profile with computed follow counts.
    public async Task<UserResponse> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if(user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // These counts are computed on each request (not stored on the User entity)
        var followersCount = await _followRepository.GetFollowersCountAsync(user.Id);
        var followingCount = await _followRepository.GetFollowingCountAsync(user.Id);

        // Map entity → DTO. Notice PasswordHash is never included in the response.
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

    // Returns users the current user doesn't follow (for the "Suggested Users" sidebar).
    public async Task<IEnumerable<UserResponse>> GetSuggestedAsync(int userId, int count)
    {
        var users = await _userRepository.GetSuggestedAsync(userId, count);
        // .Select() with lambda — maps each User entity to a UserResponse DTO
        return users.Select(u => new UserResponse
        {
            Id = u.Id,
            UserName = u.UserName,
            DisplayName = u.DisplayName,
            Email = u.Email,
            Role = u.Role,
            CreatedAt = u.CreatedAt,
            ProfileImageUrl = u.ProfileImageUrl,
            Bio = u.Bio
        });
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
