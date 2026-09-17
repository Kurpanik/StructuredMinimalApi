using Chirper.Authentication.Services;

namespace Chirper.Authentication;

public sealed record LoginHttpRequest
{
    public required string Username { get; init; }

    public required string Password { get; init; }
}

public sealed record LoginHttpResponse
{
    public required string Token { get; init; }
}

public sealed class LoginEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/login", HandleAsync)
            .WithName("Login")
            .WithSummary("Logs in a user")
            .WithRequestValidation<LoginHttpRequest>();
    }

    private static async Task<Results<Ok<LoginHttpResponse>, UnauthorizedHttpResult>> HandleAsync(
        LoginHttpRequest request,
        AppDbContext database,
        Jwt jwt,
        CancellationToken cancellationToken)
    {
        var user = await database.Users.SingleOrDefaultAsync(x => x.Username == request.Username && x.Password == request.Password, cancellationToken);

        if (user is null || user.Password != request.Password)
        {
            return TypedResults.Unauthorized();
        }

        var token = jwt.GenerateToken(user);
        var response = new LoginHttpResponse
        {
            Token = token,
        };

        return TypedResults.Ok(response);
    }
}

public sealed class LoginHttpRequestValidator : AbstractValidator<LoginHttpRequest>
{
    public LoginHttpRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
