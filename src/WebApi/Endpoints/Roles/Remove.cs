using Application.Roles.Remove;
using MediatR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Roles;

internal sealed class Remove : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/roles/remove", async (Guid roleId, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new RemoveRoleCommand(roleId);

            var result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Roles);
    }
}
