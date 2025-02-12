using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Core;
using Domain.Users;

namespace Application.Users.GetAll;

public class GetAllUsersQueryHandler(IUserRepository repository, IUserContext userContext) 
    : IQueryHandler<GetAllUsersQuery, IEnumerable<UserResponse>>
{
    public async Task<Result<IEnumerable<UserResponse>>> Handle(
        GetAllUsersQuery query, 
        CancellationToken cancellationToken)
    {
        if (userContext.UserRole != "User")
        {
            return Result.Failure<IEnumerable<UserResponse>>(UserErrors.Unauthorized());
        }
        
        var users = await repository.GetAllUsersAsync(cancellationToken);
        
        var usersResponse = users
            .AsParallel()
            .AsOrdered()
            .Select(UserResponse.Create);

        return usersResponse;
    }
}