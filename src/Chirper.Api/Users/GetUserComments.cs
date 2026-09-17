namespace Chirper.Users;

public sealed record GetUserCommentsHttpRequest : IPagedRequest
{
    public required int Id { get; init; }

    public int? Page { get; init; }

    public int? PageSize { get; init; }
}

public sealed record GetUserCommentsHttpResponse
{
    public required int Id { get; init; }

    public required int PostId { get; init; }

    public required string Content { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }

    public required int LikesCount { get; init; }

    public required int RepliesCount { get; init; }
}

public sealed class GetUserCommentsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}/comments", HandleAsync)
            .WithName("GetUserComments")
            .WithSummary("Get a user's comments")
            .WithRequestValidation<GetUserCommentsHttpRequest>()
            .WithEnsureEntityExists<User, GetUserCommentsHttpRequest>(x => x.Id);
    }

    private static async Task<Ok<PagedHttpResponse<GetUserCommentsHttpResponse>>> HandleAsync(
        [AsParameters] GetUserCommentsHttpRequest request,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var response = await database.Comments
            .Where(x => x.UserId == request.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new GetUserCommentsHttpResponse
            {
                Id = x.Id,
                PostId = x.PostId,
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

public sealed class GetUserCommentsHttpRequestValidator : PagedRequestValidator<GetUserCommentsHttpRequest>
{
    public GetUserCommentsHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
