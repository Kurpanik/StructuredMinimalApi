namespace Chirper.Users;

public sealed record GetUserFollowersHttpRequest : IPagedRequest
{
    public required int Id { get; init; }

    public int? Page { get; init; }

    public int? PageSize { get; init; }
}

public sealed record GetUserFollowersHttpResponse
{
    public required int Id { get; init; }

    public required string Username { get; init; }

    public required string Name { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}

public sealed class GetUserFollowersEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}/followers", HandleAsync)
            .WithName("GetUserFollowers")
            .WithSummary("Get a user's followers")
            .WithRequestValidation<GetUserFollowersHttpRequest>()
            .WithEnsureEntityExists<User, GetUserFollowersHttpRequest>(x => x.Id);
    }

    private static async Task<Ok<PagedHttpResponse<GetUserFollowersHttpResponse>>> HandleAsync(
        [AsParameters] GetUserFollowersHttpRequest request,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var response = await database.Follows
            .Where(x => x.FollowedUserId == request.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new GetUserFollowersHttpResponse
            {
                Id = x.FollowerUser.Id,
                Username = x.FollowerUser.Username,
                Name = x.FollowerUser.DisplayName,
                CreatedAtUtc = x.CreatedAtUtc,
            })
            .ToPagedResponseAsync(request, cancellationToken);

        return TypedResults.Ok(response);
    }
}

public sealed class GetUserFollowersHttpRequestValidator : PagedRequestValidator<GetUserFollowersHttpRequest>
{
    public GetUserFollowersHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
