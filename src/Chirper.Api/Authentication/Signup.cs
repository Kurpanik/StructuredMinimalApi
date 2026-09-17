using Chirper.Authentication.Services;

namespace Chirper.Authentication;

public sealed record SignupHttpRequest
{
    public required string Username { get; init; }

    public required string Password { get; init; }

    public required string Name { get; init; }
}

public sealed record SignupHttpResponse
{
    public required string Token { get; init; }
}

public sealed class SignupEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/signup", HandleAsync)
            .WithName("Signup")
            .WithSummary("Creates a new user account")
            .WithRequestValidation<SignupHttpRequest>();
    }

    private static async Task<Results<Ok<SignupHttpResponse>, ValidationError>> HandleAsync(
        SignupHttpRequest request,
        AppDbContext database,
        Jwt jwt,
        CancellationToken cancellationToken)
    {
        var isUsernameTaken = await database.Users
            .AnyAsync(x => x.Username == request.Username, cancellationToken);

        if (isUsernameTaken)
        {
            return new ValidationError("Username is already taken");
        }

        var user = new User
        {
            Username = request.Username,
            Password = request.Password,
            DisplayName = request.Name,
        };

        await database.Users.AddAsync(user, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);

        var token = jwt.GenerateToken(user);
        var response = new SignupHttpResponse
        {
            Token = token,
        };

        return TypedResults.Ok(response);
    }
}

public sealed class SignupHttpRequestValidator : AbstractValidator<SignupHttpRequest>
{
    public SignupHttpRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}
