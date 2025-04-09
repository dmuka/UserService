using Application.Roles.GetByUserId;
using MediatR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Roles;

internal sealed class GetRolesByUserId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/roles/user/{userId:Guid}", async (Guid userId, ISender sender, CancellationToken cancellationToken) =>
        {
            var query = new GetRolesByUserIdQuery(userId);

            var result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Roles);
    }
}
