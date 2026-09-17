namespace Chirper.Users;

public sealed record GetUserLikedPostsHttpRequest : IPagedRequest
{
    public required int Id { get; init; }

    public int? Page { get; init; }

    public int? PageSize { get; init; }
}

public sealed record GetUserLikedPostsHttpResponse
{
    public required int Id { get; init; }

    public required int UserId { get; init; }

    public string? Content { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }

    public required int LikesCount { get; init; }

    public required int CommentsCount { get; init; }

    public required DateTime LikedAtUtc { get; init; }
}

public sealed class GetUserLikedPostsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}/liked-posts", HandleAsync)
            .WithName("GetUserLikedPosts")
            .WithSummary("Get a user's liked posts")
            .WithRequestValidation<GetUserLikedPostsHttpRequest>()
            .WithEnsureEntityExists<User, GetUserLikedPostsHttpRequest>(x => x.Id);
    }

    private static async Task<Ok<PagedHttpResponse<GetUserLikedPostsHttpResponse>>> HandleAsync(
        [AsParameters] GetUserLikedPostsHttpRequest request,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var response = await database.PostLikes
            .Where(x => x.UserId == request.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new GetUserLikedPostsHttpResponse
            {
                Id = x.PostId,
                UserId = x.Post.UserId,
                Content = x.Post.Content,
                CreatedAtUtc = x.Post.CreatedAtUtc,
                UpdatedAtUtc = x.Post.UpdatedAtUtc,
                LikesCount = x.Post.Likes.Count,
                CommentsCount = x.Post.Comments.Count,
                LikedAtUtc = x.CreatedAtUtc,
            })
            .ToPagedResponseAsync(request, cancellationToken);

        return TypedResults.Ok(response);
    }
}

public sealed class GetUserLikedPostsHttpRequestValidator : PagedRequestValidator<GetUserLikedPostsHttpRequest>
{
    public GetUserLikedPostsHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
