namespace Chirper.Users;

public sealed record UnfollowUserHttpRequest
{
    public required int Id { get; init; }
}

public sealed class UnfollowUserEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}/unfollow", HandleAsync)
            .WithName("UnfollowUser")
            .WithSummary("Unfollows a user")
            .WithRequestValidation<UnfollowUserHttpRequest>()
            .WithEnsureEntityExists<User, UnfollowUserHttpRequest>(x => x.Id);
    }

    private static async Task<Results<Ok, ValidationError>> HandleAsync(
        [AsParameters] UnfollowUserHttpRequest request,
        AppDbContext database,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var userId = claimsPrincipal.GetUserId();

        if (userId == request.Id)
        {
            return new ValidationError("You cannot unfollow yourself.");
        }

        var rowsDeleted = await database.Follows
            .Where(x => x.FollowerUserId == userId && x.FollowedUserId == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (rowsDeleted == 0)
        {
            return new ValidationError("You are not following this user.");
        }

        // TODO: Publish event of an unfollow to decrement the follower count

        return TypedResults.Ok();
    }
}

public sealed class UnfollowUserHttpRequestValidator : AbstractValidator<UnfollowUserHttpRequest>
{
    public UnfollowUserHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
