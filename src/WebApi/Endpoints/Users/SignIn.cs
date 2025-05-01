using Application.Users.SignIn;
using Infrastructure.Options.Authentication;
using MediatR;
using Microsoft.Extensions.Options;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Users;

internal sealed class SignIn(IOptions<AuthOptions> authOptions) : IEndpoint
{
    public sealed record Request(string Username, string Password, string? Email = null);
    
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/users/signin", async (Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new SignInUserCommand(
                request.Username,
                request.Password,
                authOptions.Value.RefreshTokenExpirationInDays,
                request.Email);

            var result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
