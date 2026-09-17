namespace Chirper.Posts;

public sealed record UpdatePostHttpRequest
{
    public required int Id { get; init; }

    public required string Title { get; init; }

    public string? Content { get; init; }
}

public sealed class UpdatePostEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/", HandleAsync)
            .WithName("UpdatePost")
            .WithSummary("Updates a post")
            .WithRequestValidation<UpdatePostHttpRequest>()
            .WithEnsureUserOwnsEntity<Post, UpdatePostHttpRequest>(x => x.Id);
    }

    private static async Task<Ok> HandleAsync(
        UpdatePostHttpRequest request,
        AppDbContext database,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var post = await database.Posts.SingleAsync(x => x.Id == request.Id, cancellationToken);
        post.Title = request.Title;
        post.Content = request.Content;
        post.UpdatedAtUtc = DateTime.UtcNow;
        await database.SaveChangesAsync(cancellationToken);

        // TODO: Publish post updated event

        return TypedResults.Ok();
    }
}

public sealed class UpdatePostHttpRequestValidator : AbstractValidator<UpdatePostHttpRequest>
{
    public UpdatePostHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);
    }
}
