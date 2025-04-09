using Application.Users.SignUp;
using MediatR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Users;

internal sealed class SignUp : IEndpoint
{
    public sealed record Request(string Username, string Email, string FirstName, string LastName, string Password);
    
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/users/signup", async (Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new SignUpUserCommand(
                request.Username,
                request.Email,
                request.FirstName,
                request.LastName,
                request.Password);

            var result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
