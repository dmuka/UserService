using Application.Roles.GetByName;
using MediatR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Roles;

internal sealed class GetByName : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("roles/{roleName}", async (string roleName, ISender sender, CancellationToken cancellationToken) =>
        {
            var query = new GetRoleByNameQuery(roleName);

            var result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Roles);
    }
}
