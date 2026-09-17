using Chirper.Authentication;
using Chirper.Comments;
using Chirper.Posts;
using Chirper.Users;

namespace Chirper;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = app.MapGroup("");

        endpoints.MapAuthenticationEndpoints();
        endpoints.MapPostEndpoints();
        endpoints.MapCommentEndpoints();
        endpoints.MapUserEndpoints();
    }

    private static void MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/auth")
            .WithTags("Authentication");
            
        endpoints.MapPublicGroup()
            .MapEndpoint<SignupEndpoint>()
            .MapEndpoint<LoginEndpoint>();
    }

    private static void MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/posts")
            .WithTags("Posts");

        endpoints.MapPublicGroup()
            .MapEndpoint<GetPostsEndpoint>()
            .MapEndpoint<GetPostByIdEndpoint>()
            .MapEndpoint<GetPostCommentsEndpoint>();

        endpoints.MapAuthorizedGroup()
            .MapEndpoint<CreatePostEndpoint>()
            .MapEndpoint<UpdatePostEndpoint>()
            .MapEndpoint<DeletePostEndpoint>()
            .MapEndpoint<LikePostEndpoint>()
            .MapEndpoint<UnlikePostEndpoint>();
    }

    private static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/comments")
            .WithTags("Comments");

        endpoints.MapPublicGroup()
            .MapEndpoint<GetCommentRepliesEndpoint>();

        endpoints.MapAuthorizedGroup()
            .MapEndpoint<CreateCommentEndpoint>()
            .MapEndpoint<LikeCommentEndpoint>()
            .MapEndpoint<UnlikeCommentEndpoint>();
    }

    private static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/users")
            .WithTags("Users");

        endpoints.MapPublicGroup()
            .MapEndpoint<GetUserPostsEndpoint>()
            .MapEndpoint<GetUserCommentsEndpoint>()
            .MapEndpoint<GetUserFollowersEndpoint>()
            .MapEndpoint<GetUserFollowingEndpoint>()
            .MapEndpoint<GetUserLikedPostsEndpoint>()
            .MapEndpoint<GetUserLikedCommentsEndpoint>();

        endpoints.MapAuthorizedGroup()
            .MapEndpoint<FollowUserEndpoint>()
            .MapEndpoint<UnfollowUserEndpoint>();
    }

    private static RouteGroupBuilder MapPublicGroup(this IEndpointRouteBuilder app, string? prefix = null)
    {
        return app.MapGroup(prefix ?? string.Empty)
            .AllowAnonymous();
    }

    private static RouteGroupBuilder MapAuthorizedGroup(this IEndpointRouteBuilder app, string? prefix = null)
    {
        return app.MapGroup(prefix ?? string.Empty)
            .RequireAuthorization();
    }

    public static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app) where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}

public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder app);
}
