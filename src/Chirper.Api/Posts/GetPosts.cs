namespace Chirper.Posts;

public sealed record GetPostsHttpRequest : IPagedRequest
{
    public int? Page { get; init; }

    public int? PageSize { get; init; }
}

public sealed record GetPostsHttpResponse
{
    public required int Id { get; init; }

    public required string Title { get; init; }

    public string? Content { get; init; }

    public required int UserId { get; init; }

    public required string Username { get; init; }

    public required string UserDisplayName { get; init; }

    public required DateTime CreateAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }

    public required int LikesCount { get; init; }

    public required int CommentsCount { get; init; }
}

public sealed class GetPostsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/", HandleAsync)
            .WithName("GetPosts")
            .WithSummary("Gets all posts")
            .WithRequestValidation<GetPostsHttpRequest>();
    }

    private static async Task<Ok<PagedHttpResponse<GetPostsHttpResponse>>> HandleAsync(
        [AsParameters] GetPostsHttpRequest request,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var response = await database.Posts
            .Select(x => new GetPostsHttpResponse
            {
                Id = x.Id,
                Title = x.Title,
                Content = x.Content,
                UserId = x.UserId,
                Username = x.User.Username,
                UserDisplayName = x.User.DisplayName,
                CreateAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc,
                LikesCount = x.Likes.Count,
                CommentsCount = x.Comments.Count,
            })
            .ToPagedResponseAsync(request, cancellationToken);

        return TypedResults.Ok(response);
    }
}

public sealed class GetPostsHttpRequestValidator : PagedRequestValidator<GetPostsHttpRequest>;
