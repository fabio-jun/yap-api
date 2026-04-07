using Blog.Application.DTOs.Users;

namespace Blog.Application.Interfaces;

public interface IUserService
{
    // Return a users's public profile
    Task<UserResponse> GetByIdAsync(int id);
    // Search users by name
    Task<IEnumerable<UserResponse>> SearchAsync(string query);

    Task<IEnumerable<UserResponse>> GetSuggestedAsync(int userId, int count);

    //Updates the authenticated user's profile
    Task<UserResponse> UpdateAsync(int userId, UpdateUserRequest request);
}