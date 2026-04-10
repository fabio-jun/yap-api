using Blog.Application.DTOs.Users;

namespace Blog.Application.Interfaces;

// Service interface for user profile operations.
public interface IUserService
{
    // Returns a user's public profile (includes follow counts)
    Task<UserResponse> GetByIdAsync(int id);

    // Searches users by username (case-insensitive)
    Task<IEnumerable<UserResponse>> SearchAsync(string query);

    // Returns users the current user doesn't follow (for "Suggested Users" sidebar)
    Task<IEnumerable<UserResponse>> GetSuggestedAsync(int userId, int count);

    // Updates the authenticated user's own profile
    Task<UserResponse> UpdateAsync(int userId, UpdateUserRequest request);
}
