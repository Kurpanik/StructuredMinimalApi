namespace Chirper.Users;

public sealed record FollowUserHttpRequest
{
    public required int Id { get; init; }
}

public sealed class FollowUserEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/{id}/follow", HandleAsync)
            .WithName("FollowUser")
            .WithSummary("Follows a user")
            .WithRequestValidation<FollowUserHttpRequest>()
            .WithEnsureEntityExists<User, FollowUserHttpRequest>(x => x.Id);
    }

    private static async Task<Results<Ok, ValidationError>> HandleAsync(
        [AsParameters] FollowUserHttpRequest request,
        AppDbContext database,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var userId = claimsPrincipal.GetUserId();

        if (userId == request.Id)
        {
            return new ValidationError("You cannot follow yourself.");
        }

        var isAlreadyFollowing = await database.Follows
            .AnyAsync(x => x.FollowerUserId == userId && x.FollowedUserId == request.Id, cancellationToken);

        if (isAlreadyFollowing)
        {
            return new ValidationError("You are already following this user.");
        }

        var follow = new Follow
        {
            FollowerUserId = userId,
            FollowedUserId = request.Id,
        };

        // TODO: Send a notification to the user being followed

        await database.Follows.AddAsync(follow, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok();
    }
}

public sealed class FollowUserHttpRequestValidator : AbstractValidator<FollowUserHttpRequest>
{
    public FollowUserHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
