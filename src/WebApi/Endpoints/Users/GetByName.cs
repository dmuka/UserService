using Application.Users.GetByName;
using MediatR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Users;

internal sealed class GetByName : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/users/{userName}", async (string userName, ISender sender, CancellationToken cancellationToken) =>
        {
            var query = new GetUserByNameQuery(userName);

            var result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
