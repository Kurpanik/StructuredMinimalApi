namespace Chirper.Posts;

public sealed record DeletePostHttpRequest
{
    public required int Id { get; init; }
}

public sealed class DeletePostEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}", HandleAsync)
            .WithName("DeletePost")
            .WithSummary("Deletes a post")
            .WithRequestValidation<DeletePostHttpRequest>()
            .WithEnsureUserOwnsEntity<Post, DeletePostHttpRequest>(x => x.Id);
    }

    private static async Task<Results<Ok, NotFound>> HandleAsync(
        [AsParameters] DeletePostHttpRequest request,
        AppDbContext database,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var rowsDeleted = await database.Posts
            .Where(x => x.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        return rowsDeleted == 1
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }
}

public sealed class DeletePostHttpRequestValidator : AbstractValidator<DeletePostHttpRequest>
{
    public DeletePostHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
