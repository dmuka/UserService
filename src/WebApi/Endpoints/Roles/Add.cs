using Application.Roles.Add;
using MediatR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Roles;

internal sealed class Add : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/roles/add", async (string name, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new AddRoleCommand(name);

            var result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Roles);
    }
}
