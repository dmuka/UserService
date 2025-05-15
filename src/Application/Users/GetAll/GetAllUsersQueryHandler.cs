using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Users;

namespace Application.Users.GetAll;

public class GetAllUsersQueryHandler(
    IUserRepository repository,
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

        var response = users.Select(user => new UserResponse
        {
            Id = user.Id.Value,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = user.RoleNames.Select(name => name.Value).ToArray(),
            IsMfaEnabled = user.IsMfaEnabled ? "yes" : "no",
            PasswordHash = user.PasswordHash
        }).ToList();
        
        return response;
    }
}