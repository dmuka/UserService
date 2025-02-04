using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Users.GetById;
using Core;
using Domain.Users;

namespace Application.Users.GetByName;

public class GetUserByNameQueryHandler(IUserRepository repository, IUserContext userContext) 
    : IQueryHandler<GetUserByNameQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(
        GetUserByNameQuery query, 
        CancellationToken cancellationToken)
    {
        if (query.UserName != userContext.UserName)
        {
            return Result.Failure<UserResponse>(UserErrors.Unauthorized());
        }
        
        var user = await repository.GetUserByUsernameAsync(query.UserName, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound(query.UserName));
        }
        
        var userResponse = UserResponse.Create(user);

        return userResponse;
    }
}