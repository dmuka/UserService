using Application.Roles.GetAll;
using MediatR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Roles;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("roles", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var query = new GetAllRolesQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Roles);
    }
}
