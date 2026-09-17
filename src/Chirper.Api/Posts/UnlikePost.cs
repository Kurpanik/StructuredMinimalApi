namespace Chirper.Posts;

public sealed record UnlikePostHttpRequest
{
    public required int Id { get; init; }
}

public sealed class UnlikePostEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}/unlike", HandleAsync)
            .WithName("UnlikePost")
            .WithSummary("Unlikes a post")
            .WithRequestValidation<UnlikePostHttpRequest>()
            .WithEnsureEntityExists<Post, UnlikePostHttpRequest>(x => x.Id);
    }

    private static async Task<Results<Ok, NotFound>> HandleAsync(
        [AsParameters] UnlikePostHttpRequest request,
        AppDbContext database,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var userId = claimsPrincipal.GetUserId();

        var rowsDeleted = await database.PostLikes
            .Where(x => x.PostId == request.Id && x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        return rowsDeleted == 0
            ? TypedResults.NotFound()
            : TypedResults.Ok();
    }
}

public sealed class UnlikePostHttpRequestValidator : AbstractValidator<UnlikePostHttpRequest>
{
    public UnlikePostHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
