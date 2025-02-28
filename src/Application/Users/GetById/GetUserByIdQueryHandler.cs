using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Roles;
using Domain.Users;

namespace Application.Users.GetById;

public class GetUserByIdQueryHandler(
    IUserRepository repository, 
    IRoleRepository roleRepository,
    IUserContext userContext) 
    : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(
        GetUserByIdQuery query, 
        CancellationToken cancellationToken)
    {
        if (query.UserId != userContext.UserId)
        {
            return Result.Failure<UserResponse>(UserErrors.Unauthorized());
        }
        
        var user = await repository.GetUserByIdAsync(query.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound(query.UserId));
        }
        
        var roles = await roleRepository.GetRolesByUserIdAsync(query.UserId, cancellationToken);
        
        var userResponse = UserResponse.Create(user, roles.Select(role => role.Name).ToArray());

        return userResponse;
    }
}