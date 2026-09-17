namespace Chirper.Users;

public sealed record GetUserLikedCommentsHttpRequest : IPagedRequest
{
    public required int Id { get; init; }

    public int? Page { get; init; }

    public int? PageSize { get; init; }
}

public sealed record GetUserLikedCommentsHttpResponse
{
    public required int Id { get; init; }

    public required int PostId { get; init; }

    public required int UserId { get; init; }

    public required string Username { get; init; }

    public required string DisplayName { get; init; }

    public required string Content { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }

    public required int LikesCount { get; init; }

    public required int RepliesCount { get; init; }

    public required DateTime LikedAtUtc { get; init; }
}

public sealed class GetUserLikedCommentsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}/liked-comments", HandleAsync)
            .WithName("GetUserLikedComments")
            .WithSummary("Get a user's liked comments")
            .WithRequestValidation<GetUserLikedCommentsHttpRequest>()
            .WithEnsureEntityExists<User, GetUserLikedCommentsHttpRequest>(x => x.Id);
    }

    private static async Task<Ok<PagedHttpResponse<GetUserLikedCommentsHttpResponse>>> HandleAsync(
        [AsParameters] GetUserLikedCommentsHttpRequest request,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var response = await database.CommentLikes
            .Where(x => x.UserId == request.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new GetUserLikedCommentsHttpResponse
            {
                Id = x.CommentId,
                PostId = x.Comment.PostId,
                UserId = x.Comment.UserId,
                Username = x.Comment.User.Username,
                DisplayName = x.Comment.User.DisplayName,
                Content = x.Comment.Content,
                CreatedAtUtc = x.Comment.CreatedAtUtc,
                UpdatedAtUtc = x.Comment.UpdatedAtUtc,
                LikesCount = x.Comment.Likes.Count,
                RepliesCount = x.Comment.Replies.Count,
                LikedAtUtc = x.CreatedAtUtc,
            })
            .ToPagedResponseAsync(request, cancellationToken);

        return TypedResults.Ok(response);
    }
}

public sealed class GetUserLikedCommentsHttpRequestValidator : PagedRequestValidator<GetUserLikedCommentsHttpRequest>
{
    public GetUserLikedCommentsHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
