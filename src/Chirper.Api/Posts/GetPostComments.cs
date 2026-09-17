namespace Chirper.Posts;

public sealed record GetPostCommentsHttpRequest : IPagedRequest
{
    public required int Id { get; init; }

    public int? Page { get; init; }

    public int? PageSize { get; init; }
}

public sealed record GetPostCommentsHttpResponse
{
    public required int Id { get; init; }

    public required int UserId { get; init; }

    public required string Username { get; init; }

    public required string DisplayName { get; init; }

    public required string Content { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }

    public required int LikesCount { get; init; }

    public required int RepliesCount { get; init; }
}

public sealed class GetPostCommentsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}/comments", HandleAsync)
            .WithName("GetPostComments")
            .WithSummary("Get a post's top level comments")
            .WithRequestValidation<GetPostCommentsHttpRequest>()
            .WithEnsureEntityExists<Post, GetPostCommentsHttpRequest>(x => x.Id);
    }

    private static async Task<Ok<PagedHttpResponse<GetPostCommentsHttpResponse>>> HandleAsync(
        [AsParameters] GetPostCommentsHttpRequest request,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var response = await database.Comments
            .Where(x => x.PostId == request.Id && x.ReplyToCommentId == null)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new GetPostCommentsHttpResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                Username = x.User.Username,
                DisplayName = x.User.DisplayName,
                Content = x.Content,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc,
                LikesCount = x.Likes.Count,
                RepliesCount = x.Replies.Count,
            })
            .ToPagedResponseAsync(request, cancellationToken);

        return TypedResults.Ok(response);
    }
}

public sealed class GetPostCommentsHttpRequestValidator : PagedRequestValidator<GetPostCommentsHttpRequest>
{
    public GetPostCommentsHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
