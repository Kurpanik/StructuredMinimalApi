namespace Chirper.Comments;

public sealed record GetCommentRepliesHttpRequest : IPagedRequest
{
    public required int Id { get; init; }

    public int? Page { get; init; }

    public int? PageSize { get; init; }
}

public sealed record GetCommentRepliesHttpResponse
{
    public required int Id { get; init; }

    public required int UserId { get; init; }

    public required string Username { get; init; }

    public required string UserDisplayName { get; init; }

    public required string Content { get; init; }

    public required int NumberOfReplies { get; init; }
}

public sealed class GetCommentRepliesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}/replies", HandleAsync)
            .WithName("GetCommentReplies")
            .WithSummary("Gets all replies to a comment")
            .WithRequestValidation<GetCommentRepliesHttpRequest>()
            .WithEnsureEntityExists<Comment, GetCommentRepliesHttpRequest>(x => x.Id);
    }

    private static async Task<Ok<PagedHttpResponse<GetCommentRepliesHttpResponse>>> HandleAsync(
        [AsParameters] GetCommentRepliesHttpRequest request,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var response = await database.Comments
            .Where(x => x.ReplyToCommentId == request.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new GetCommentRepliesHttpResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                Username = x.User.Username,
                UserDisplayName = x.User.DisplayName,
                Content = x.Content,
                NumberOfReplies = x.Replies.Count,
            })
            .ToPagedResponseAsync(request, cancellationToken);

        return TypedResults.Ok(response);
    }
}

public sealed class GetCommentRepliesHttpRequestValidator : PagedRequestValidator<GetCommentRepliesHttpRequest>
{
    public GetCommentRepliesHttpRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
