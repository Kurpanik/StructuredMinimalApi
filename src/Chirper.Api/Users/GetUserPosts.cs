namespace Chirper.Users;

public sealed record GetUserPostsHttpRequest : IPagedRequest
{
    public required int Id { get; init; }

    public int? Page { get; init; }

    public int? PageSize { get; init; }
}

public sealed record GetUserPostsHttpResponse
{
    public required int Id { get; init; }

    public required string Title { get; init; }

    public string? Content { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }

    public required int LikesCount { get; init; }

    public required int CommentsCount { get; init; }
}

public sealed class GetUserPostsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}/posts", HandleAsync)
            .WithName("GetUserPosts")
            .WithSummary("Get a user's posts")
            .WithRequestValidation<GetUserPostsHttpRequest>()
            .WithEnsureEntityExists<User, GetUserPostsHttpRequest>(x => x.Id);
    }

    private static async Task<Ok<PagedHttpResponse<GetUserPostsHttpResponse>>> HandleAsync(
        [AsParameters] GetUserPostsHttpRequest request,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var response = await database.Posts
            .Where(x => x.UserId == request.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new GetUserPostsHttpResponse
            {
                Id = x.Id,
                Title = x.Title,
                Content = x.Content,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc,
                LikesCount = x.Likes.Count,
                CommentsCount = x.Comments.Count,
            })
            .ToPagedResponseAsync(request, cancellationToken);

        return TypedResults.Ok(response);
    }
}

public sealed class GetUserPostsHttpRequestValidator : PagedRequestValidator<GetUserPostsHttpRequest>
{
    public GetUserPostsHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
