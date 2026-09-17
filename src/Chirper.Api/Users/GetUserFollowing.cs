namespace Chirper.Users;

public sealed record GetUserFollowingHttpRequest : IPagedRequest
{
    public required int Id { get; init; }

    public int? Page { get; init; }

    public int? PageSize { get; init; }
}

public sealed record GetUserFollowingHttpResponse
{
    public required int Id { get; init; }

    public required string Username { get; init; }

    public required string DisplayName { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}

public sealed class GetUserFollowingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}/following", HandleAsync)
            .WithName("GetUserFollowing")
            .WithSummary("Get a user's following list")
            .WithRequestValidation<GetUserFollowingHttpRequest>()
            .WithEnsureEntityExists<User, GetUserFollowingHttpRequest>(x => x.Id);
    }

    private static async Task<Ok<PagedHttpResponse<GetUserFollowingHttpResponse>>> HandleAsync(
        [AsParameters] GetUserFollowingHttpRequest request,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var response = await database.Follows
            .Where(x => x.FollowerUserId == request.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new GetUserFollowingHttpResponse
            {
                Id = x.FollowedUserId,
                Username = x.FollowedUser.Username,
                DisplayName = x.FollowedUser.DisplayName,
                CreatedAtUtc = x.CreatedAtUtc,
            })
            .ToPagedResponseAsync(request, cancellationToken);

        return TypedResults.Ok(response);
    }
}

public sealed class GetUserFollowingHttpRequestValidator : PagedRequestValidator<GetUserFollowingHttpRequest>
{
    public GetUserFollowingHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
