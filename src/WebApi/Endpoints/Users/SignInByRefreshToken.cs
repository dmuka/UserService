using Application.Users.SignIn;
using Application.Users.SignInByToken;
using MediatR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Users;

internal sealed class SignInByRefreshToken : IEndpoint
{
    public sealed record Request(string RefreshToken);
    public sealed record Response(string AccessToken, string RefreshToken);
    
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/users/signinbytoken", async (Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new SignInUserByTokenCommand(request.RefreshToken);

            var result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
