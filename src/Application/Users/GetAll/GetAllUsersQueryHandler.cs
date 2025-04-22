using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Roles;
using Domain.Users;

namespace Application.Users.GetAll;

public class GetAllUsersQueryHandler(
    IUserRepository repository,
    IRoleRepository roleRepository,
    IUserContext userContext) 
    : IQueryHandler<GetAllUsersQuery, IList<UserResponse>>
{
    public async Task<Result<IList<UserResponse>>> Handle(
        GetAllUsersQuery query, 
        CancellationToken cancellationToken)
    {
        if (userContext.UserRole != "Admin")
        {
            return Result.Failure<IList<UserResponse>>(UserErrors.Unauthorized());
        }
        
        var users = await repository.GetAllUsersAsync(cancellationToken);

        var usersResponse = await Task.WhenAll(
            users.Select(async user =>
            {
                var roles = await roleRepository.GetRolesByUserIdAsync(user.Id.Value, cancellationToken);

                return UserResponse.Create(user, roles.Select(role => role.Name).ToArray());
            }));

        return usersResponse;
    }
}