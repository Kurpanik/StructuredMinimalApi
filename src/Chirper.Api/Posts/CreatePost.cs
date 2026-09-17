namespace Chirper.Posts;

public sealed record CreatePostHttpRequest
{
    public required string Title { get; init; }

    public string? Content { get; init; }
}

public sealed record CreatePostHttpResponse
{
    public required int Id { get; init; }
}

public sealed class CreatePostEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/", HandleAsync)
            .WithName("CreatePost")
            .WithSummary("Creates a new post")
            .WithRequestValidation<CreatePostHttpRequest>();
    }

    private static async Task<Ok<CreatePostHttpResponse>> HandleAsync(
        CreatePostHttpRequest request,
        AppDbContext database,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            UserId = claimsPrincipal.GetUserId(),
        };

        await database.Posts.AddAsync(post, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);

        var response = new CreatePostHttpResponse
        {
            Id = post.Id,
        };

        return TypedResults.Ok(response);
    }
}

public sealed class CreatePostHttpRequestValidator : AbstractValidator<CreatePostHttpRequest>
{
    public CreatePostHttpRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);
    }
}
