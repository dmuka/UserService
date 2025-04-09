using Application.Roles.GetById;
using MediatR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Roles;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/roles/{roleId:Guid}", async (Guid roleId, ISender sender, CancellationToken cancellationToken) =>
        {
            var query = new GetRoleByIdQuery(roleId);

            var result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Roles);
    }
}
