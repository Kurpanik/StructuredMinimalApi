namespace Chirper.Comments;

public sealed record LikeCommentHttpRequest
{
    public required int Id { get; init; }
}

public sealed class LikeCommentEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/{id}/like", HandleAsync)
            .WithName("LikeComment")
            .WithSummary("Like a comment")
            .WithRequestValidation<LikeCommentHttpRequest>()
            .WithEnsureEntityExists<Comment, LikeCommentHttpRequest>(x => x.Id);
    }

    private static async Task<Ok> HandleAsync(
        [AsParameters] LikeCommentHttpRequest request,
        AppDbContext database,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var userId = claimsPrincipal.GetUserId();

        var exists = await database.CommentLikes
            .AnyAsync(x => x.CommentId == request.Id && x.UserId == userId, cancellationToken);

        if (exists)
        {
            return TypedResults.Ok();
        }

        var like = new CommentLike
        {
            CommentId = request.Id,
            UserId = userId,
        };

        await database.CommentLikes.AddAsync(like, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok();
    }
}

public sealed class LikeCommentHttpRequestValidator : AbstractValidator<LikeCommentHttpRequest>
{
    public LikeCommentHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
