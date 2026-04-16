using Blog.Application.Cache;

namespace Blog.Tests;

public class CacheKeysTests
{
    [Fact]
    public void CacheKeys_UseStableFormats()
    {
        Assert.Equal("posts:all", CacheKeys.AllPosts());
        Assert.Equal("feed:42", CacheKeys.UserFeed(42));
        Assert.Equal("user:7:profile", CacheKeys.UserProfile(7));
        Assert.Equal("user:7:suggested", CacheKeys.SuggestedUsers(7));
        Assert.Equal("tags:all", CacheKeys.AllTags());
        Assert.Equal("notifications:7", CacheKeys.Notifications(7));
        Assert.Equal("user:", CacheKeys.UserPrefix);
        Assert.Equal("posts:", CacheKeys.PostsPrefix);
    }
}
