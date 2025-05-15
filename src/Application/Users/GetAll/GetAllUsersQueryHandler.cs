using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Users;

namespace Application.Users.GetAll;

public class GetAllUsersQueryHandler(
    IUserRepository repository,
    IUserContext userContext) 
    : IQueryHandler<GetAllUsersQuery, IList<User>>
{
    public async Task<Result<IList<User>>> Handle(
        GetAllUsersQuery query, 
        CancellationToken cancellationToken)
    {
        if (userContext.UserRole != "Admin")
        {
            return Result.Failure<IList<User>>(UserErrors.Unauthorized());
        }
        
        var users = await repository.GetAllUsersAsync(cancellationToken);

        return users.ToList();
    }
}