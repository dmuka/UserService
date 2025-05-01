using Application.Users.SignInByToken;
using Infrastructure.Options.Authentication;
using MediatR;
using Microsoft.Extensions.Options;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Users;

internal sealed class SignInByRefreshToken(IOptions<AuthOptions> authOptions) : IEndpoint
{
    public sealed record Request(string RefreshToken);
    public sealed record Response(string AccessToken, string SessionId);
    
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/users/signinbytoken", async (Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new SignInUserByTokenCommand(request.RefreshToken, authOptions.Value.RefreshTokenExpirationInDays);

            var result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
