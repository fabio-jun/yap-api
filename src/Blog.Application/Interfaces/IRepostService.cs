using Blog.Application.DTOs.Reposts;

namespace Blog.Application.Interfaces;

public interface IRepostService
{
    Task<RepostStateResponse> ToggleAsync(int postId, int userId);
    Task<RepostStateResponse> QuoteAsync(int postId, QuoteRepostRequest request, int userId);
    Task<RepostStateResponse> GetStateAsync(int postId, int? userId);
}
