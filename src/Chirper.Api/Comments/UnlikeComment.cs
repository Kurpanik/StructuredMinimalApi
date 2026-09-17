namespace Chirper.Comments;

public sealed record UnlikeCommentHttpRequest
{
    public required int Id { get; init; }
}

public sealed class UnlikeCommentEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}/unlike", HandleAsync)
            .WithName("UnlikeComment")
            .WithSummary("Unlike a comment")
            .WithRequestValidation<UnlikeCommentHttpRequest>()
            .WithEnsureEntityExists<Comment, UnlikeCommentHttpRequest>(x => x.Id);
    }

    private static async Task<Results<Ok, NotFound>> HandleAsync(
        [AsParameters] UnlikeCommentHttpRequest request,
        AppDbContext database,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var userId = claimsPrincipal.GetUserId();

        var rowsDeleted = await database.CommentLikes
            .Where(x => x.CommentId == request.Id && x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        if (rowsDeleted == 0)
        {
            return TypedResults.NotFound();
        }

        // TODO: Publish event to notify comment unliked

        return TypedResults.Ok();
    }
}

public sealed class UnlikeCommentHttpRequestValidator : AbstractValidator<UnlikeCommentHttpRequest>
{
    public UnlikeCommentHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
