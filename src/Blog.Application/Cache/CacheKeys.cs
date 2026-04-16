namespace Blog.Application.Cache;

public static class CacheKeys
{
    public static string AllPosts() => "posts:all";
    public static string UserFeed(int userId) => $"feed:{userId}";
    public static string UserProfile(int id) => $"user:{id}:profile";
    public static string SuggestedUsers(int userId) => $"user:{userId}:suggested";
    public static string AllTags() => "tags:all";
    public static string Notifications(int userId) => $"notifications:{userId}";

    public const string UserPrefix = "user:";
    public const string PostsPrefix = "posts:";
}
