namespace Chirper.Posts;

public sealed record LikePostHttpRequest
{
    public required int Id { get; init; }
}

public sealed class LikePostEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/{id}/like", HandleAsync)
            .WithName("LikePost")
            .WithSummary("Likes a post")
            .WithRequestValidation<LikePostHttpRequest>()
            .WithEnsureEntityExists<Post, LikePostHttpRequest>(x => x.Id);
    }

    private static async Task<Ok> HandleAsync(
        [AsParameters] LikePostHttpRequest request,
        AppDbContext database,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var userId = claimsPrincipal.GetUserId();
        var doesLikeExist = await database.PostLikes.AnyAsync(x => x.PostId == request.Id && x.UserId == userId, cancellationToken);

        if (doesLikeExist)
        {
            return TypedResults.Ok();
        }

        var like = new PostLike
        {
            PostId = request.Id,
            UserId = userId,
        };

        await database.PostLikes.AddAsync(like, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok();
    }
}

public sealed class LikePostHttpRequestValidator : AbstractValidator<LikePostHttpRequest>
{
    public LikePostHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
