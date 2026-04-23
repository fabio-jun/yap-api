using System.Reflection;
using Blog.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.Tests;

public class SwaggerDocumentationTests
{
    [Fact]
    public void ProtectedEndpoints_UseAuthorizeAttribute()
    {
        Assert.Null(typeof(AuthController).GetMethod(nameof(AuthController.Login))!
            .GetCustomAttribute<AuthorizeAttribute>());
        Assert.NotNull(typeof(PostController).GetMethod(nameof(PostController.Create))!
            .GetCustomAttribute<AuthorizeAttribute>());
    }

    [Fact]
    public void PostGetAll_Documentation_MatchesCurrentDescription()
    {
        var action = typeof(PostController).GetMethod(nameof(PostController.GetAll))!;
        var attribute = action.GetCustomAttribute<SwaggerOperationAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal(
            "Returns yaps. Parameter precedence is search, then tag, then page. pageSize is only applied when page is provided.",
            attribute!.Description);
    }

    [Fact]
    public void UsersSearch_Documentation_MatchesCurrentDescription()
    {
        var action = typeof(UsersController).GetMethod(nameof(UsersController.Search))!;
        var attribute = action.GetCustomAttribute<SwaggerOperationAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("Searches users by username or display name.", attribute!.Description);
    }

    [Fact]
    public void Register_Documentation_MatchesAuthResponsePayload()
    {
        var action = typeof(AuthController).GetMethod(nameof(AuthController.Register))!;
        var attribute = action.GetCustomAttribute<SwaggerOperationAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal(
            "Creates a user account and returns an access token, refresh token, and expiration timestamp.",
            attribute!.Description);
    }
}
