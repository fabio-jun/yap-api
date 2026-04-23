using Blog.Application.DTOs.Mentions;

namespace Blog.Application.DTOs.Posts;

// DTO for returning post data to the client.
// Contains flattened data from Post + User + Like + Bookmark entities.
// This avoids exposing the internal entity structure and circular references.
public class PostResponse
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Flattened from Post.Author — avoids sending the entire User entity
    public required int AuthorId { get; set; }
    public required string AuthorName { get; set; }
    public string? AuthorProfileImageUrl { get; set; }

    // Media URL from object storage
    public string? ImageUrl { get; set; }

    // Aggregated data — computed per-request in the service layer
    public int LikeCount { get; set; }

    // Whether the currently authenticated user has liked/bookmarked this post
    // These are user-specific — different for each viewer
    public bool HasLiked { get; set; }
    public bool HasBookmarked { get; set; }
    public int RepostCount { get; set; }
    public bool HasReposted { get; set; }

    public bool IsRepost { get; set; }
    public int? RepostId { get; set; }
    public int? RepostedByUserId { get; set; }
    public string? RepostedByUserName { get; set; }
    public string? RepostedByProfileImageUrl { get; set; }
    public DateTime? RepostedAt { get; set; }
    public string? QuoteContent { get; set; }
    public int OriginalPostId { get; set; }
    public List<MentionedUserResponse> MentionedUsers { get; set; } = [];
}
