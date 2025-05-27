using Application.Users.SignInBySessionId;
using MediatR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Users;

internal sealed class SignInBySessionId : IEndpoint
{
    public sealed record Request(string SessionId);
    public sealed record Response(string AccessToken);
    
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost(
                "/api/users/signinbysessionid", 
                async (Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new SignInUserBySessionIdCommand(Guid.Parse(request.SessionId));

            var result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
