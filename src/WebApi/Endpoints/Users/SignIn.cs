using Application.Users.SignIn;
using MediatR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Users;

internal sealed class SignIn : IEndpoint
{
    public sealed record Request(string Username, string Password, string? Email = null);
    
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("users/signin", async (Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new SignInUserCommand(
                request.Username,
                request.Password,
                request.Email);

            var result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
