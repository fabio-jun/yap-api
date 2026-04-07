using Blog.Domain.Interfaces;
using Blog.Application.DTOs.Users;
using Blog.Application.Interfaces;

namespace Blog.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IFollowRepository _followRepository;

    public UserService(IUserRepository userRepository, IFollowRepository followRepository)
    {
        _userRepository = userRepository;
        _followRepository = followRepository;
    }

    // Receives an usr ID and returns a userResponse DTO
    public async Task<UserResponse> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if(user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var followersCount = await _followRepository.GetFollowersCountAsync(user.Id);
        var followingCount = await _followRepository.GetFollowingCountAsync(user.Id);

        return new UserResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            ProfileImageUrl = user.ProfileImageUrl,
            Bio = user.Bio,
            FollowersCount = followersCount,
            FollowingCount = followingCount
        };
    }

    public async Task<IEnumerable<UserResponse>> GetSuggestedAsync(int userId, int count)
    {
        var users = await _userRepository.GetSuggestedAsync(userId, count);
        return users.Select(u => new UserResponse
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            Role = u.Role,
            CreatedAt = u.CreatedAt,
            ProfileImageUrl = u.ProfileImageUrl,
            Bio = u.Bio
        });
    }

    // Searches users by username
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
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                Bio = user.Bio,
                FollowersCount = await _followRepository.GetFollowersCountAsync(user.Id),
                FollowingCount = await _followRepository.GetFollowingCountAsync(user.Id)
            });
        }

        return responses;
    }

    // Updates user profile and return the updated UserResponse DTO
    public async Task<UserResponse> UpdateAsync(int userId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if(user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        user.UserName = request.userName;
        user.Email = request.Email;
        user.ProfileImageUrl = request.ProfileImageUrl;
        user.Bio = request.Bio;

        await _userRepository.UpdateAsync(user);

        var followersCount = await _followRepository.GetFollowersCountAsync(user.Id);
        var followingCount = await _followRepository.GetFollowingCountAsync(user.Id);

        return new UserResponse
        {
            Id = user.Id,
            UserName = user.UserName,
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