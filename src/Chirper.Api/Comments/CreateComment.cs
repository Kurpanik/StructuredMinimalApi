namespace Chirper.Comments;

public sealed record CreateCommentHttpRequest
{
    public required int PostId { get; init; }

    public required string Content { get; init; }

    public int? ReplyToCommentId { get; init; }
}

public sealed record CreateCommentHttpResponse
{
    public required int Id { get; init; }
}

public sealed class CreateCommentEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/", HandleAsync)
            .WithName("CreateComment")
            .WithSummary("Creates a new comment")
            .WithRequestValidation<CreateCommentHttpRequest>()
            .WithEnsureEntityExists<Post, CreateCommentHttpRequest>(x => x.PostId)
            .WithEnsureEntityExists<Comment, CreateCommentHttpRequest>(x => x.ReplyToCommentId);
    }

    private static async Task<Ok<CreateCommentHttpResponse>> HandleAsync(
        CreateCommentHttpRequest request,
        AppDbContext database,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var comment = new Comment
        {
            PostId = request.PostId,
            UserId = claimsPrincipal.GetUserId(),
            Content = request.Content,
            ReplyToCommentId = request.ReplyToCommentId,
        };

        await database.Comments.AddAsync(comment, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);

        var response = new CreateCommentHttpResponse
        {
            Id = comment.Id,
        };

        return TypedResults.Ok(response);
    }
}

public sealed class CreateCommentHttpRequestValidator : AbstractValidator<CreateCommentHttpRequest>
{
    public CreateCommentHttpRequestValidator()
    {
        RuleFor(x => x.PostId).GreaterThan(0);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.ReplyToCommentId).GreaterThan(0);
    }
}
