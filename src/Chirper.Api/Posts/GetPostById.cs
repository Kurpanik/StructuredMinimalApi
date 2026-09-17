namespace Chirper.Posts;

public sealed record GetPostByIdHttpRequest
{
    public required int Id { get; init; }
}

public sealed record GetPostByIdHttpResponse
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

public sealed class GetPostByIdEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}", HandleAsync)
            .WithName("GetPostById")
            .WithSummary("Gets a post by id")
            .WithRequestValidation<GetPostByIdHttpRequest>();
    }

    private static async Task<Results<Ok<GetPostByIdHttpResponse>, NotFound>> HandleAsync(
        [AsParameters] GetPostByIdHttpRequest request,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var post = await database.Posts
            .Where(x => x.Id == request.Id)
            .Select(x => new GetPostByIdHttpResponse
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
            .SingleOrDefaultAsync(cancellationToken);

        return post is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(post);
    }
}

public sealed class GetPostByIdHttpRequestValidator : AbstractValidator<GetPostByIdHttpRequest>
{
    public GetPostByIdHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
