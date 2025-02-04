using Application.Abstractions.Messaging;

namespace Application.Users.GetByName;

public sealed record GetUserByNameQuery(string UserName) : IQuery<UserResponse>;