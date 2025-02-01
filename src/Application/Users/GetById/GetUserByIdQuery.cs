using Application.Abstractions.Messaging;

namespace Application.Users.GetById;

public sealed record GetUserByIdQuery(long UserId) : IQuery<UserResponse>;