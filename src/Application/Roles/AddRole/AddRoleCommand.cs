using Application.Abstractions.Messaging;
using Core;

namespace Application.Roles.AddRole;

public sealed record AddRoleCommand(string Name) : ICommand<Guid>;