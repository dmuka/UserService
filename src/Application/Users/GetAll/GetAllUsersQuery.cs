using Application.Abstractions.Messaging;
using Domain.Users;

namespace Application.Users.GetAll;

public sealed record GetAllUsersQuery : IQuery<IList<User>>;